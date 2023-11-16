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

        public int timeToStartRound = 10;

        #endregion

        #region Member Variables

        [HideInInspector] public NetworkVariable<bool> isRoundStarted = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        [HideInInspector] public NetworkVariable<bool> isRoundOver = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        public static RoundManager Instance { get; private set; }

        //private const int MaxPlayers = 10;
        private const int TimeToRespawn = 5;
        private int m_timeRemainingToStartRound;

        #endregion

        #region Events

        public static event Action OnRoundStarted;

        #endregion

        #region InitData

        private void Awake()
        {
            ManageSingleton();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("RoundManager spawned");
            if (IsServer)
            {
                GetReferences();
                if (!isRoundStarted.Value)
                {
                    InitServer();
                    StartRoundServerRpc();
                }
            }

            SoundManager.Instance.StartBackgroundMusic(SoundManager.BackgroundMusic.InGame);
        }

        public void InitServer()
        {
            m_timeRemainingToStartRound = timeToStartRound;
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
        void StartRoundServerRpc(ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("Round started");
            GameManager.Instance.OnStartGameServerRpc();
            OnRoundStarted?.Invoke();
            StartCoroutine(StartRound());

            IEnumerator StartRound()
            {
                m_timeRemainingToStartRound--;
                yield return new WaitForSeconds(timeToStartRound);
                isRoundStarted.Value = true;
            }
        }

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
                }
                catch (Exception e)
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
            return this.Cameras.Find(camera => camera.CompareTag("PlayerFPSCamera"))
                .GetComponent<CinemachineVirtualCamera>();
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