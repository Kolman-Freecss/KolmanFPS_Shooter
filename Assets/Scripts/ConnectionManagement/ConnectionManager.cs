#region

using ConnectionManagement.ConnectionState._impl._common;
using Gameplay.Config;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace ConnectionManagement
{
    /// <summary>
    /// Every client has a ConnectionManager. This class is responsible for managing the connection state of the client.
    /// 
    /// This state machine handles connection through the NetworkManager. It is responsible for listening to
    /// NetworkManger callbacks and other outside calls and redirecting them to the current ConnectionState object.
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {
        #region Member Variables

        private ConnectionState.ConnectionState m_CurrentState;

        public static ConnectionManager Instance { get; private set; }

        public int MaxPlayers = 10;
        public int NbReconnectAttempts = 2;

        #endregion

        #region InitData

        private void Awake()
        {
            ManageSingleton();
        }

        /**
         * <summary>Manage the singleton pattern for this class (Object destroyed when changing scene)</summary>
         */
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

        private void Start()
        {
            m_CurrentState = new OfflineState(Instance);
            SubscribeToServerEvents();
        }

        void SubscribeToServerEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        }

        #endregion

        #region Logic

        public void ChangeState(ConnectionState.ConnectionState nextState)
        {
            Debug.Log($"ConnectionManager: Changing state from {m_CurrentState} to {nextState}");
            if (m_CurrentState != null)
            {
                m_CurrentState.Exit();
            }

            m_CurrentState = nextState;
            m_CurrentState.Enter();
        }

        //TODO: Implement ConnectionApproval Checks
        /// <summary>
        /// When a client connects, we apply the player prefab to the client through this filter
        /// </summary>
        /// <param name="connectionApprovalRequest"></param>
        /// <param name="connectionApprovalResponse"></param>
        // private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
        //     NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
        // {
        //     // var playerPrefabIndex = System.BitConverter.ToInt32(connectionApprovalRequest.Payload);
        //     var payload = System.BitConverter.ToInt32(connectionApprovalRequest.Payload);
        //     Entities.Player.Player.TeamType teamType = (Entities.Player.Player.TeamType) payload;
        //     uint playerPrefabSelection = GameManager.Instance.SkinsGlobalNetworkIds.Find(skin => skin.Key == teamType).Value;
        //     connectionApprovalResponse.PlayerPrefabHash = playerPrefabSelection;
        // }
        public void OnServerStarted()
        {
            m_CurrentState.OnServerStarted();
        }

        public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            m_CurrentState.ApprovalCheck(request, response);
        }

        public void StartHost(string playerName, string ipAddress, int port)
        {
            m_CurrentState.StartHostIP(playerName, ipAddress, port);
        }

        public void StartClient(string playerName, string ipAddress, int port)
        {
            m_CurrentState.StartClientIP(playerName, ipAddress, port);
        }

        public void OnTransportFailure()
        {
            m_CurrentState.OnTransportFailure();
        }

        /// <summary>
        /// Callback called when a client connects
        /// </summary>
        /// <param name="clientId"></param>
        void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");
            m_CurrentState.OnClientConnected(clientId);
        }

        /// <summary>
        /// Callback called when a client disconnects
        /// </summary>
        /// <param name="clientId"></param>
        void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
            m_CurrentState.OnClientDisconnect(clientId);
            //GameManager.Instance.OnClientDisconnectCallbackServerRpc(clientId);
        }

        public void Disconnect(ulong clientId)
        {
            // Check if the clientId that wants to disconnect is the host
            if (clientId == NetworkManager.ServerClientId)
            {
                // If the host disconnects, the server will be stopped
                NetworkManager.Singleton.Shutdown(false);
            }
            else
            {
                // If a client disconnects, the server will be notified
                GameManager.Instance.OnClientDisconnectCallbackServerRpc(clientId);
                NetworkManager.Singleton.DisconnectClient(clientId, "Client disconnected.");
            }
        }

        #endregion

        #region Destructor

        private void OnDestroy()
        {
            UnsubscribeToServerEvents();
        }

        void UnsubscribeToServerEvents()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
                NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
                NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
            }
        }

        #endregion
    }
}