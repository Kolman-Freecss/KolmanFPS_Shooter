using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Config
{
    public class ConnectionManager : MonoBehaviour
    {
        
        #region Auxiliary Variables

        public static ConnectionManager Instance { get; private set; }
        
        [HideInInspector]
        public List<NetworkClient> clients;

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
            GetReferences();
        }
        
        void GetReferences()
        {
            if (clients == null) clients = new List<NetworkClient>();
        }

        #endregion
        
        #region Logic
        
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }
        
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
        
        public void AddClient(NetworkClient client)
        {
            if (!clients.Contains(client))
            {
                clients.Add(client);
            }
        }
        
        public void RemoveClient(NetworkClient client)
        {
            if (clients.Contains(client))
            {
                clients.Remove(client);
            }
        }
        
        public void RemoveAllClients()
        {
            clients.Clear();
        }
        
        #endregion
        
    }
}