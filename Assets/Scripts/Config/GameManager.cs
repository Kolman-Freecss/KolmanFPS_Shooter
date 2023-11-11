using System.Collections;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace Config
{
    public class GameManager : NetworkBehaviour
    {
        #region Member properties

        public static GameManager Instance { get; private set; }
        
        [HideInInspector]
        public NetworkVariable<bool> isGameStarted = new NetworkVariable<bool>(false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);
        
        [HideInInspector]
        public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);
        
        private const int TimeToEndGame = 5;

        #endregion

        #region InitData

        void Awake()
        {
            Assert.IsNull(Instance, $"Multiple instances of {nameof(Instance)} detected. This should not happen.");
            ManageSingleton();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
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
                //Server will notified to a single client when his scene is loaded
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] {clientId}
                    }
                };
                OnClientConnectedCallbackClientRpc(clientId, clientRpcParams);
            }
        }
        
        private void StartGame()
        {
            if (!isGameStarted.Value && SceneTransitionHandler.Instance.GetCurrentSceneState().Equals(SceneTransitionHandler.SceneStates.Multiplayer_InGame))
            {
                Debug.Log("------------------START GAME------------------");
                isGameStarted.Value = true;
            }
        }
        
        public void PlayerEndGame(ulong clientId)
        {
            SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_EndGame);
            OnPlayerEndGameServerRpc();
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
            PlayerEndGame(clientId);
            RemovePlayerFromGameClientRpc(clientId);
        }
        
        [ClientRpc]
        private void OnClientConnectedCallbackClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            /*if (IsOwner) return;*/
            Debug.Log("------------------SENT Client Loaded Scene------------------");
            Debug.Log("Client Id -> " + clientId);
            StartGame();
        }
        
        [ServerRpc]
        public void OnClientDisconnectCallbackServerRpc(ulong cliendId, ServerRpcParams serverRpcParams = default)
        {
            RemovePlayerFromGameClientRpc(cliendId);
        }

        [ClientRpc]
        private void RemovePlayerFromGameClientRpc(ulong cliendId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------ Player removed------------------ " + cliendId);
            RemovePlayer(cliendId);
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