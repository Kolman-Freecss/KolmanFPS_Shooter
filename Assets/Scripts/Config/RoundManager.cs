using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Weapons;
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

        #region Member Variables

        public static RoundManager Instance { get; private set; }
        //private const int MaxPlayers = 10;
        private const int TimeToRespawn = 5;

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
            if (IsServer)
            {
                GetReferences();
                OnRoundManagerSpawned?.Invoke();
            }
            SoundManager.Instance.StartBackgroundMusic(SoundManager.BackgroundMusic.InGame);
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

        #region Network Events Handler
        
        [ServerRpc]
        private void DetachWeaponsFromPlayerServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default)
        {
            NetworkManager.Singleton.ConnectedClients[clientId].OwnedObjects.ForEach((netObj) =>
            {
                Weapon weapon = netObj.GetComponent<Weapon>();
                if (weapon != null)
                {
                    DetachWeaponFromPlayer(weapon.gameObject);    
                }
            });
            
            void DetachWeaponFromPlayer(GameObject weapon)
            {
                try
                {
                    PositionConstraint pc = weapon.GetComponent<PositionConstraint>();
                    pc.enabled = false;
                    RotationConstraint rc = weapon.GetComponent<RotationConstraint>();
                    rc.enabled = false;
                    weapon.GetComponent<Rigidbody>().useGravity = true;
                    weapon.GetComponent<BoxCollider>().enabled = true;
                    // Change ownership of the weapon to the server
                    weapon.GetComponent<NetworkObject>().RemoveOwnership();
                } catch (Exception e)
                {
                    Debug.LogError("Error detaching weapon from player: " + e.Message);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnPlayerDeathServerRpc(ulong networkObjectPlayerId, ServerRpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            OnPlayerDeathClientRpc(clientId);
            StartCoroutine(OnPlayerDeath(clientId));
            
            IEnumerator OnPlayerDeath(ulong clientId, float timeToRespawn = TimeToRespawn)
            {
                yield return new WaitForSeconds(timeToRespawn);
                DetachWeaponsFromPlayerServerRpc(clientId);
                RespawnClientRpc(clientId);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        public void RespawnClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            //TODO: Make the respawn not GAME OVER
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                GameManager.Instance.OnPlayerEndGameServerRpc();
            } 
            // else
            // {
            //     Debug.Log("Player " + clientId + " respawned");
            // }
        }
        
        [ClientRpc]
        public void OnPlayerDeathClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("You died");
            }
            else
            {
                Debug.Log("Player " + clientId + " died");
            }
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
        
        public void OnDestroy()
        {
            base.OnDestroy();
            Debug.Log("RoundManager destroyed");
        }

        #endregion
        
    }
}