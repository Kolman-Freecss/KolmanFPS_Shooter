#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities.Player.Skin;
using Entities.Utils;
using Gameplay.Player;
using Modules.CacheModule;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

#endregion

namespace Gameplay.Config
{
    public class GameManager : NetworkBehaviour
    {
        #region Member properties

        public static GameManager Instance { get; private set; }

        [HideInInspector] public NetworkVariable<bool> isGameStarted = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [HideInInspector] public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [HideInInspector] public NetworkVariable<bool> allPlayersReady = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        //TODO: Change this to a list of players
        [HideInInspector] public NetworkVariable<int> quantityPlayersInGame = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private const int TimeToEndGame = 5;

        private readonly string PlayerSkinsPath = "Player/Skins";

        private List<GameObject> m_Skins = new List<GameObject>();

        public List<GameObject> Skins => m_Skins;

        public List<SerializableDictionaryEntry<Entities.Player.Player.TeamType, uint>> SkinsGlobalNetworkIds;

        public Dictionary<Entities.Player.Player.TeamType, List<GameObject>> SkinsByTeam =
            new Dictionary<Entities.Player.Player.TeamType, List<GameObject>>();

        private CacheManagement m_CacheManagement;

        public CacheManagement CacheManagement => m_CacheManagement;

        public event Action<ulong> OnGameStarted;

        public event Action allPlayersSpawned;

        #endregion

        #region InitData

        void Awake()
        {
            Assert.IsNull(Instance, $"Multiple instances of {nameof(Instance)} detected. This should not happen.");
            ManageSingleton();
            if (SkinsGlobalNetworkIds == null || SkinsGlobalNetworkIds.Count == 0)
            {
                Assert.IsNotNull(SkinsGlobalNetworkIds, "SkinsGlobalNetworkIds is null or empty");
            }

            if (m_Skins == null || m_Skins.Count == 0)
            {
                List<GameObject> m_Skins = Resources.LoadAll<GameObject>(PlayerSkinsPath).ToList();
                m_Skins.ForEach(skin =>
                {
                    NetworkObject networkObject = skin.GetComponent<NetworkObject>();
                    PlayerSkin playerSkin = skin.GetComponentInChildren<PlayerSkin>();
                    if (networkObject != null)
                    {
                        if (SkinsByTeam.ContainsKey(playerSkin.TeamSkinValue))
                        {
                            SkinsByTeam[playerSkin.TeamSkinValue].Add(skin);
                        }
                        else
                        {
                            SkinsByTeam.Add(playerSkin.TeamSkinValue, new List<GameObject>() { skin });
                        }
                    }
                    else Debug.LogWarning("Skin " + skin.name + " has no NetworkObject component");
                    // if (networkObject != null) m_SkinsGlobalNetworkIds.Add(playerSkin.TeamSkinValue, networkObject.PrefabIdHash);
                    // else Debug.LogWarning("Skin " + skin.name + " has no NetworkObject component");
                });
            }
        }

        private void Start()
        {
            m_CacheManagement = new CacheManagement();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                SceneTransitionHandler.Instance.OnClientLoadedGameScene += ClientLoadedGameScene;

                Init();
            }

            //NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void Init()
        {
            isGameStarted.Value = false;
            isGameOver.Value = false;
            allPlayersReady.Value = false;
            quantityPlayersInGame.Value = 0;
        }

        private void ManageSingleton()
        {
            if (Instance != null)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }

        #endregion

        #region Logic

        private void ClientLoadedGameScene(ulong clientId)
        {
            if (IsServer)
            {
                CheckClientsInScene(clientId);
                //Server will notified to a single client when his scene is loaded
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                OnClientConnectedCallbackClientRpc(clientId, clientRpcParams);
            }
        }

        private void CheckClientsInScene(ulong clientId)
        {
            Debug.Log("New client in scene -> " + clientId);
            int totalClients = NetworkManager.Singleton.ConnectedClients.Count;
            // int clientsInScene = 0;
            // foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.Singleton.ConnectedClients)
            // {
            //     if (client.Value.PlayerObject != null)
            //     {
            //         clientsInScene++;
            //     }
            // }

            quantityPlayersInGame.Value++;

            if (totalClients == quantityPlayersInGame.Value)
            {
                Debug.Log("------------------ALL CLIENTS IN GAME SCENE------------------");
                allPlayersReady.Value = true;
            }
            else
            {
                Debug.Log("Total clients -> " + totalClients + " | Clients in scene -> " + quantityPlayersInGame.Value);
            }
        }

        [ClientRpc]
        public void PlayerDeathClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            Debug.Log("------------------YOU DEAD------------------");
            NetworkManager.Singleton.Shutdown();
            SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_EndGame, false);
        }

        public void AddPlayer(ulong clientId, PlayerController player)
        {
        }

        public void RemovePlayer(ulong clientId)
        {
        }

        #endregion

        #region Network calls/Events

        /// <summary>
        /// When the game starts the server will notify all clients to start the game and will spawn all players
        /// </summary>
        /// <param name="serverRpcParams"></param>
        [ServerRpc]
        public void OnStartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            SpawnAllPlayersServerRpc();
            bool sub = false;
            if (allPlayersReady.Value) StartGame();
            else
            {
                sub = true;
                allPlayersReady.OnValueChanged += StartGame;
            }
            //NotifyAllPlayersClientRpc();

            void StartGame(bool oldValue = false, bool newValue = true)
            {
                if (!isGameStarted.Value && SceneTransitionHandler.Instance.GetCurrentSceneState()
                        .Equals(SceneTransitionHandler.SceneStates.Multiplayer_InGame))
                {
                    Debug.Log("------------------START GAME------------------");
                    allPlayersSpawned?.Invoke();
                    isGameStarted.Value = true;
                    OnGameStarted?.Invoke(NetworkManager.Singleton.LocalClientId);
                    // If we subscribe to the event, we need to unsubscribe here
                    if (sub)
                        allPlayersReady.OnValueChanged -= StartGame;
                }
            }
        }

        /// <summary>
        /// Spawn all the networkObjects player prefab for all players
        /// </summary>
        /// <param name="serverRpcParams"></param>
        [ServerRpc]
        private void SpawnAllPlayersServerRpc(ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("------------------SPAWN PLAYERS------------------");
            List<ulong> connectedPlayers = NetworkManager.Singleton.ConnectedClientsIds.ToList();
            for (var i = 0; i < connectedPlayers.Count; i++)
            {
                ulong clientId = connectedPlayers[i];
                //TODO: Get the team from the player data
                // Team 1 = i % 2 == 0
                // Team 2 = i % 2 != 0
                Entities.Player.Player.TeamType teamType = i % 2 == 0
                    ? Entities.Player.Player.TeamType.Warriors
                    : Entities.Player.Player.TeamType.Wizards;
                GameObject playerGo = Instance.SkinsByTeam[teamType][0];
                GameObject player = Instantiate(playerGo);
                NetworkObject noPlayer = player.GetComponent<NetworkObject>();
                // Make this noPlayer PlayerObject for this client
                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject = noPlayer;
                Debug.Log("Player spawned -> " + clientId);
                noPlayer.SpawnWithOwnership(clientId, true);
                // SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId,
                //     new SessionPlayerData(clientId, "Player " + clientId, 0, true, true, teamType));
            }

            Debug.Log("------------------ALL PLAYERS SPAWNED------------------");
        }

        /// <summary>
        /// When the game is over, the server will notify all clients to end the game
        /// </summary>
        [ServerRpc]
        public void OnEndGameServerRpc()
        {
            OnEndGameClientRpc();
            //NetworkManager.Singleton.StopHost();
        }

        [ClientRpc]
        public void OnEndGameClientRpc()
        {
            if (!isGameOver.Value)
            {
                Debug.Log("------------------END GAME------------------");
                isGameOver.Value = true;
                EndGame();
            }

            IEnumerator EndGame(int timeToEndGame = TimeToEndGame)
            {
                yield return new WaitForSeconds(timeToEndGame);
                SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_EndGame);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnPlayerEndGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            if (clientId != NetworkManager.ServerClientId)
            {
                //ConnectionManager.Instance.Disconnect(clientId);
                PlayerDeathClientRpc(clientId);
                OnClientDisconnectCallbackServerRpc(clientId);
            }
        }

        [ClientRpc]
        private void OnClientConnectedCallbackClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Loaded Scene------------------");
            Debug.Log("Client Id -> " + clientId);
            //StartGame();
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnClientDisconnectCallbackServerRpc(ulong cliendId, ServerRpcParams serverRpcParams = default)
        {
            RemovePlayerFromGameClientRpc(cliendId);
        }

        [ClientRpc]
        private void RemovePlayerFromGameClientRpc(ulong cliendId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------ Player removed------------------ " + cliendId);
            //RemovePlayer(cliendId);
        }

        #endregion

        #region Destructor

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer)
            {
                UnregisterServerCallbacks();
            }

            ClearInitData();
            UnSubscribeToDelegatesAndUpdateValues();
        }

        public void ClearInitData()
        {
        }

        private void UnregisterServerCallbacks()
        {
            SceneTransitionHandler.Instance.OnClientLoadedGameScene -= ClientLoadedGameScene;
        }

        void UnSubscribeToDelegatesAndUpdateValues()
        {
        }

        #endregion
    }
}