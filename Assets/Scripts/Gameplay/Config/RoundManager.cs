#region

using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Config;
using Gameplay.Weapons;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

#endregion

namespace Gameplay.Config
{
    public class RoundManager : NetworkBehaviour
    {
        //TODO: Build checkpoint entity

        #region Inspector Variables

        public TextMeshProUGUI TimeToStartRoundText;
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

        [HideInInspector] public NetworkVariable<int> m_timeRemainingToStartRound = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        private bool _isRoundStarting = false;

        #endregion

        #region Events

        public static event Action OnRoundStarted;
        public event Action OnRoundManagerSpawned;

        #endregion

        #region InitData

        private void Awake()
        {
            ManageSingleton();
        }

        public override void OnNetworkSpawn()
        {
            Debug.Log("RoundManager spawned");
            OnRoundManagerSpawned?.Invoke();
            // if (IsServer)
            //     //GetReferences();
            //     if (!isRoundStarted.Value)
            //     {
            //         StartRoundServerRpc();
            //     }
            if (IsServer)
                InitServer();

            SoundManager.Instance.StartBackgroundMusic(SoundManager.BackgroundMusic.InGame);
        }

        public void InitServer()
        {
            m_timeRemainingToStartRound.Value = timeToStartRound;
        }

        private void Start()
        {
            GetReferences();
            if (IsServer)
                if (!isRoundStarted.Value)
                    StartRoundServerRpc();
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

        private void GetReferences()
        {
            if (_checkpoints == null) _checkpoints = new List<GameObject>();
            _isRoundStarting = false;
            StartingRoundClientRpc(false);
        }

        #endregion

        #region Loop

        private void Update()
        {
            if (!isRoundStarted.Value && _isRoundStarting)
                TimeToStartRoundText.text = m_timeRemainingToStartRound.Value.ToString();
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
            Debug.Log("Round ready");
            GameManager.Instance.OnGameStarted += InitRound;

            // When the round starts, we need to start the game server side
            GameManager.Instance.OnStartGameServerRpc();

            void InitRound(ulong serverClientId)
            {
                Debug.Log("Init Round");
                OnRoundStarted?.Invoke();
                StartingRoundClientRpc(true);
                StartCoroutine(StartRound(timeToStartRound));

                IEnumerator StartRound(int time)
                {
                    int timeRemaining = time;
                    while (timeRemaining > 0)
                    {
                        m_timeRemainingToStartRound.Value = timeRemaining;
                        yield return new WaitForSeconds(1);
                        timeRemaining--;
                    }

                    StartingRoundClientRpc(false);
                    isRoundStarted.Value = true;
                }
            }
        }

        [ClientRpc]
        void StartingRoundClientRpc(bool isRoundStarting, ClientRpcParams clientRpcParams = default)
        {
            if (isRoundStarting)
            {
                TimeToStartRoundText.gameObject.SetActive(true);
            }
            else
            {
                TimeToStartRoundText.gameObject.SetActive(false);
            }

            _isRoundStarting = isRoundStarting;
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