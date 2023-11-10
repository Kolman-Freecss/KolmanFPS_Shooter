using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Config
{
    public class GameManager : NetworkBehaviour
    {
        #region Auxiliar properties

        public static GameManager Instance { get; private set; }
        
        [HideInInspector]
        public NetworkVariable<bool> isGameStarted = new NetworkVariable<bool>(false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);
        
        [HideInInspector]
        public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);
        
        Dictionary<ulong, PlayerController> players = new Dictionary<ulong, PlayerController>();

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
            players.Clear();
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
            if (!isGameStarted.Value && SceneTransitionHandler.Instance.GetCurrentSceneState().Equals(SceneTransitionHandler.SceneStates.InGame))
            {
                Debug.Log("------------------START GAME------------------");
                isGameStarted.Value = true;
            }
        }

        #endregion

        #region Network calls/Events
        
        [ClientRpc]
        private void OnClientConnectedCallbackClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            /*if (IsOwner) return;*/
            Debug.Log("------------------SENT Client Loaded Scene------------------");
            Debug.Log("Client Id -> " + clientId);
            StartGame();
        }

        // <summary>
        // Called when a client connects to the server
        // TODO: Manage it from the server
        // </summary>
        public void AddPlayer(ulong networkObjectId, PlayerController player)
        {
            players.Add(networkObjectId, player);
        }
        
        
        // <summary>
        // Called when a client disconnects from the server
        // TODO: Manage it from the server
        // </summary>
        public void RemovePlayerAllClients(ulong networkObjectId)
        {
            //Server will notified to a single client when his scene is loaded
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] {networkObjectId}
                }
            };
            RemovePlayerFromGameClientRpc(networkObjectId);
        }
        
        [ClientRpc]
        private void RemovePlayerFromGameClientRpc(ulong networkObjectId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------ Player removed------------------ " + networkObjectId);
            RemovePlayer(networkObjectId);
        }
        
        public void RemovePlayer(ulong networkObjectId)
        {
            players.Remove(networkObjectId);
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
            players.Clear();
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