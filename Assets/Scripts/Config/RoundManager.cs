using System;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Config
{
    public class RoundManager : NetworkBehaviour
    {
        //TODO: Build checkpoint entity
        
        #region Inspector Variables

        public List<GameObject> _checkpoints;
        public List<GameObject> Cameras;
        public GameObject WeaponPool;

        #endregion

        #region Auxiliary Variables

        public static RoundManager Instance { get; private set; }

        #endregion

        #region Events

        public static event Action OnRoundManagerSpawned;

        #endregion

        #region InitData

        private void Awake()
        {
            ManageSingleton();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                GetReferences();
                OnRoundManagerSpawned?.Invoke();
            }
        }
        
        private void Start()
        {
            GetReferences();
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

        #region Getter & Setter

        public UnityEngine.Camera GetMainCamera()
        {
            return this.Cameras.Find(camera => camera.CompareTag("MainCamera")).GetComponent<UnityEngine.Camera>();
        }
        
        public CinemachineVirtualCamera GetPlayerFPSCamera()
        {
            return this.Cameras.Find(camera => camera.CompareTag("PlayerFPSCamera")).GetComponent<CinemachineVirtualCamera>();
        }

        #endregion

        #region Destructor

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            Debug.Log("RoundManager despawned");
        }

        #endregion
        
    }
}