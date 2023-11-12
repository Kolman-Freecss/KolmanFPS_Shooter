using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Config
{
    public class ConnectionManager : MonoBehaviour
    {
        
        #region Auxiliary Variables

        public static ConnectionManager Instance { get; private set; }
        
        // TODO: Add client list to manage clients
        // [HideInInspector]
        // public List<NetworkClient> clients;
        
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
            if (NetworkManager.Singleton.IsServer)
            {
                SubscribeToServerEvents();
            }
        }
        
        void SubscribeToServerEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }

        #endregion
        
        #region Logic
        
        public void StartHost(string ipAddress, int port)
        {
            try
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, (ushort) port);
                if (NetworkManager.Singleton.StartHost())
                {
                    SceneTransitionHandler.Instance.RegisterNetworkCallbacks();
                    SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_InGame);
                }
                else
                {
                    Debug.LogError("Host failed to start");
                }
            } catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        public void StartClient(string ipAddress, int port)
        {
            try
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipAddress, (ushort) port);
                if (NetworkManager.Singleton.StartClient())
                {
                    Debug.Log("Client started");
                }
                else
                {
                    Debug.LogWarning("Client failed to start");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        /// <summary>
        /// Callback called when a client connects
        /// </summary>
        /// <param name="clientId"></param>
        void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");
        }
        
        /// <summary>
        /// Callback called when a client disconnects
        /// </summary>
        /// <param name="clientId"></param>
        void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
            GameManager.Instance.OnClientDisconnectCallbackServerRpc(clientId);
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
            }
        }

        #endregion
        
    }
}