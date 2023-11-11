using System;
using System.Collections.Generic;
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
            SubscribeToEvents();
        }
        
        void SubscribeToEvents()
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
                    Debug.Log("Host started");
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
                    Debug.Log("Client failed to start");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
        
        void OnClientConnected(ulong clientId)
        {
            Debug.Log($"Client {clientId} connected");
        }
        
        void OnClientDisconnect(ulong clientId)
        {
            Debug.Log($"Client {clientId} disconnected");
            GameManager.Instance.OnClientDisconnectCallbackServerRpc(clientId);
        }
        
        #endregion

        #region Destructor

        private void OnDisable()
        {
            UnsubscribeToEvents();
        }
        
        void UnsubscribeToEvents()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        #endregion
        
    }
}