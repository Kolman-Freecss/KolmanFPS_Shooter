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
            GetReferences();
        }
        
        void GetReferences()
        {
        }

        #endregion
        
        #region Logic
        
        public void StartHost()
        {
            try
            {
                if (NetworkManager.Singleton.StartHost())
                {
                    SceneTransitionHandler.Instance.RegisterNetworkCallbacks();
                    SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.InGame);
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
        
        public void StartClient()
        {
            try
            {
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
        
        #endregion
        
    }
}