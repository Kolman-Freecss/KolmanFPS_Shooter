using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Config
{
    public class RoundManager : NetworkBehaviour
    {
        //TODO: Build checkpoint entity
        
        #region Inspector Variables

        public List<GameObject> _checkpoints;

        #endregion

        #region Auxiliary Variables

        public static RoundManager Instance { get; private set; }

        #endregion

        #region InitData

        public override void OnNetworkSpawn()
        {
            Debug.Log("RoundManager spawned");
            base.OnNetworkSpawn();
            if (IsServer)
            {
                Debug.Log("RoundManager is server");
                GetReferences();
            }
        }

        private void Awake()
        {
            Debug.Log("RoundManager awake");
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
            }
        }

        private void Start()
        {
            GetReferences();
        }
        
        void GetReferences()
        {
            if (_checkpoints == null) _checkpoints = new List<GameObject>();
        }

        #endregion

        #region Logic

        public GameObject GetRandomCheckpoint()
        {
            return _checkpoints[Random.Range(0, _checkpoints.Count)];
        }

        #endregion
    }
}