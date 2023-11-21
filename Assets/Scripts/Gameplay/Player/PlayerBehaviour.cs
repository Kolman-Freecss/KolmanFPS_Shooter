#region

using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Config;
using Gameplay.GameplayObjects;
using Gameplay.Weapons;
using Model;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;

#endregion

namespace Gameplay.Player
{
    public class PlayerBehaviour : NetworkBehaviour
    {
        #region Inspector variables

        [Header("Player")] [Tooltip("Max health of the player")] [SerializeField]
        private float _maxHealth = 100f;

        [Header("Weapons")] [Tooltip("BasePlayer weapon of the player")] [SerializeField]
        private Weapon _defaultWeapon;

        #endregion

        #region Member Variables

        PlayerInputController _playerInputController;
        PlayerController _playerController;
        [HideInInspector] public PlayerController PlayerController => _playerController;


        // Player State
        NetworkLifeState _networkLifeState;
        [HideInInspector] public LifeState LifeState => _networkLifeState.LifeState.Value;
        DamageReceiver _damageReceiver;
        [HideInInspector] public DamageReceiver DamageReceiver => _damageReceiver;
        private float _currentHealth = 100f;
        List<NetworkObject> _weapons = new List<NetworkObject>();
        Weapon _currentWeapon;
        [HideInInspector] public Weapon CurrentWeapon => _currentWeapon;
        int _currentWeaponIndex = 0;

        //TODO: Move this to another class
        // Canvas state
        private TextMeshProUGUI _healthText;
        private TextMeshProUGUI _ammoText;

        #endregion

        #region InitData

        /// <summary>
        /// Only Get own GameObject components
        /// </summary>
        private void Awake()
        {
            _networkLifeState = GetComponent<NetworkLifeState>();
            _damageReceiver = GetComponent<DamageReceiver>();
            _playerController = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
        }

        public override void OnNetworkSpawn()
        {
            _damageReceiver.DamageReceived += OnDamageReceived;

            if (IsOwner)
            {
                _networkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
            }
        }

        /// <summary>
        /// Invoked when the object is instantiated, we need to wait for the scene to load
        /// </summary>
        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Init default values
        /// </summary>
        void Init()
        {
            _currentWeaponIndex = 0;
            _currentHealth = _maxHealth;
        }

        void InitRoundPlayers(ulong clientId)
        {
            if (IsServer)
            {
                Debug.Log("InitRoundPlayers -> " + NetworkObjectId + " " + NetworkManager.Singleton.LocalClientId +
                          " " + IsOwner);
                SendClientInitDataClientRpc(NetworkManager.Singleton.LocalClientId);
            }
        }

        /// <summary>
        /// Invoked when a client loaded the game scene
        /// </summary>
        /// <param name="clientId"></param>
        void ClientLoadedGameScene(ulong clientId)
        {
            if (IsServer)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                SendClientInitDataClientRpc(clientId, clientRpcParams);
            }
        }

        #endregion

        #region Loop

        private void Update()
        {
            if (!RoundManager.Instance.isRoundStarted.Value ||
                LifeState != LifeState.Alive)
                return;
            UpdatePlayerCanvas();
            Shoot();
        }

        #endregion

        #region Logic

        void OnLifeStateChanged(LifeState prevLifeState, LifeState lifeState)
        {
            switch (lifeState)
            {
                case LifeState.Alive:
                    //TODO: Play some sound or animation like LETS GO
                    break;
                case LifeState.Dead:
                    //TODO: Clear actions queue and play some animation and sound
                    OnDead();
                    break;
            }

            void OnDead()
            {
                enabled = false;
                _playerController.OnDead();
                //TODO: Respawn or wait to finish the current round
                //TODO: Detach all weapons from player
            }
        }

        void OnDamageReceived(PlayerBehaviour inflicter, int damage)
        {
            //NetworkObject networkObjectReceiver = NetworkManager.Singleton.SpawnManager.SpawnedObjects[NetworkObjectId];
            TakeDamageServerRpc(NetworkObjectId, inflicter.NetworkObjectId, damage);
            // if (_networkLifeState.LifeState.Value.Equals(LifeState.Dead))
            // {
            //     //TODO: Plus kill to the inflicter
            //     // AddKillToPlayerServerRpc(inflicter.NetworkObjectId);
            // }
        }

        void UpdatePlayerCanvas()
        {
            this._healthText.text = _currentHealth.ToString();
            if (_currentWeapon != null)
            {
                this._ammoText.text = _currentWeapon.currentAmmo.getAmmoInfo();
            }
        }

        void Shoot()
        {
            if (_playerInputController != null && _playerInputController.leftClick)
            {
                if (_currentWeapon == null)
                {
                    //TODO: Make some kind of melee attack
                    return;
                }

                _currentWeapon.Shoot();
            }
        }


        void SwitchWeapon()
        {
            // if (_playerInputController.mouseWheelUp)
            // {
            //     _currentWeaponIndex++;
            //     if (_currentWeaponIndex > _weapons.Count - 1)
            //     {
            //         _currentWeaponIndex = 0;
            //     }
            // }
            // else if (_playerInputController.mouseWheelDown)
            // {
            //     _currentWeaponIndex--;
            //     if (_currentWeaponIndex < 0)
            //     {
            //         _currentWeaponIndex = _weapons.Count - 1;
            //     }
            // }
        }

        /// <summary>
        /// We call server to spawn the weapon and then we equip it
        /// </summary>
        /// <param name="weaponType"></param>
        void EquipWeapon(WeaponType weaponType)
        {
            EquipWeaponServerRpc((int)weaponType, NetworkManager.Singleton.LocalClientId);
        }

        #endregion

        #region Network Calls/Events

        /// <summary>
        /// The server need to know when the client perform an action over anything 
        /// </summary>
        /// <param name="receiverNetworkObjectId"></param>
        /// <param name="inflicterNetworkObjectId"></param>
        /// <param name="damage"></param>
        /// <param name="serverRpcParams"></param>
        [ServerRpc(RequireOwnership = false)]
        private void TakeDamageServerRpc(ulong receiverNetworkObjectId, ulong inflicterNetworkObjectId, int damage,
            ServerRpcParams serverRpcParams = default)
        {
            ulong clientIdReceiver = NetworkManager.Singleton.SpawnManager.SpawnedObjects[receiverNetworkObjectId]
                .OwnerClientId;
            TakeDamageClientRpc(clientIdReceiver, inflicterNetworkObjectId, damage);
        }

        [ClientRpc]
        private void TakeDamageClientRpc(ulong clientId, ulong inflicterNetworkObjectId, int damage,
            ClientRpcParams clientRpcParams = default)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            PlayerBehaviour playerBehaviour = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerBehaviour>();
            playerBehaviour._currentHealth -= damage;
            if (playerBehaviour._currentHealth <= 0)
            {
                playerBehaviour._currentHealth = 0;
                playerBehaviour._networkLifeState.LifeState.Value = LifeState.Dead;
            }
        }

        /// <summary>
        /// Call to send the init data to the spawned client on the game scene
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        private void SendClientInitDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Behaviour init data ------------------");
            Debug.Log("Client Id -> " + clientId + " - " + NetworkManager.Singleton.LocalClientId + " - " + IsOwner +
                      " - " + IsLocalPlayer);

            if (!IsOwner) return;
            clientId = NetworkManager.Singleton.LocalClientId;
            Debug.Log("Client Id changed -> " + clientId + " - " + NetworkManager.Singleton.LocalClientId + " - " +
                      IsOwner +
                      " - " + IsLocalPlayer);

            InitRoundData();
        }

        public void InitRoundData()
        {
            try
            {
                //Get Components
                PlayerBehaviour playerBehaviour = GetComponent<PlayerBehaviour>();
                //NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerBehaviour>();
                playerBehaviour._playerInputController = playerBehaviour.GetComponent<PlayerInputController>();
                playerBehaviour._playerController = playerBehaviour.GetComponent<PlayerController>();
                //Player Canvas
                GameObject c = GameObject.FindGameObjectWithTag("PlayerCanvas");
                playerBehaviour._healthText = c.transform.Find("HealthWrapper").transform.Find("HealthText")
                    .GetComponent<TextMeshProUGUI>();
                playerBehaviour._ammoText = c.transform.Find("AmmoText").GetComponent<TextMeshProUGUI>();
                //Equip Weapon
                if (playerBehaviour._defaultWeapon != null)
                {
                    EquipWeapon(playerBehaviour._defaultWeapon.weaponType);
                }
                else
                {
                    EquipWeapon(WeaponType.Ak47);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error on InitRoundData: " + e.Message);
            }
        }

        /// <summary>
        /// Called to set the current weapon active on the client
        /// </summary>
        /// <param name="networkObjectId"></param>
        [ServerRpc(RequireOwnership = false)]
        public void SetClientWeaponActiveServerRpc(ulong networkObjectId, bool active = true)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            if (no != null)
            {
                no.gameObject.SetActive(active);
            }
        }


        [ServerRpc(RequireOwnership = false)]
        void EquipWeaponServerRpc(int weaponTypeReference, ulong clientId, ServerRpcParams serverRpcParams = default)
        {
            WeaponType weaponType = (WeaponType)weaponTypeReference;
            String path = "Weapon/" + weaponType.ToString();
            GameObject weaponPrefab = Resources.Load<GameObject>(path);
            if (weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab, RoundManager.Instance.WeaponPool.transform);
                NetworkObject no = weaponInstance.GetComponent<NetworkObject>();
                no.SpawnWithOwnership(clientId);
                no.transform.localPosition = weaponPrefab.transform.position;
                no.transform.localRotation = Quaternion.identity;
                no.transform.localScale = weaponPrefab.transform.localScale;
                no.gameObject.SetActive(false);
                // TODO: Set the weapon as a child of tthe RoundManager.WeaponPool
                //no.transform.SetParent(RoundManager.Instance.WeaponPool.transform);
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId }
                    }
                };
                AttachSpawnedWeaponClientRpc(clientId, no.NetworkObjectId, clientRpcParams);
            }
            else
            {
                Debug.LogError("Weapon prefab not found at path: " + path);
            }
        }

        /// <summary>
        /// Invoked from server to attach the spawned weapon to the player 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="networkObjectId"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        private void AttachSpawnedWeaponClientRpc(ulong clientId, ulong networkObjectId,
            ClientRpcParams clientRpcParams = default)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) return;
            // We need to get the player object from the client that called the server because the server invoked the method from his own NetworkObject
            // NetworkObject player = NetworkManager.Singleton.LocalClient.PlayerObject;
            Weapon weapon = FindObjectsOfType<Weapon>(includeInactive: true).FirstOrDefault(w =>
            {
                NetworkObject wNo = w.GetComponent<NetworkObject>();
                return wNo != null && wNo.NetworkObjectId == networkObjectId;
            });
            if (weapon == null)
            {
                Debug.LogError("Weapon not found");
                return;
            }

            NetworkObject no = weapon.GetComponent<NetworkObject>();
            if (no == null)
            {
                Debug.LogError("Weapon networkObject not found");
                return;
            }

            // NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            PlayerBehaviour playerBehaviour = GetComponent<PlayerBehaviour>();
            playerBehaviour._weapons.Add(no);
            weapon.AttachToPlayer(this);
            try
            {
                PositionConstraint pc = weapon.GetComponent<PositionConstraint>();
                if (pc)
                {
                    pc.AddSource(new ConstraintSource()
                    {
                        sourceTransform = playerBehaviour._playerController.Player.RightHand,
                        weight = 1
                    });
                    pc.AddSource(new ConstraintSource()
                    {
                        sourceTransform = playerBehaviour._playerController.Player.LeftHand,
                        weight = 1
                    });
                    // Modify the constraint settings
                    pc.locked = false;
                    // pc.translationOffset = new Vector3(-0.03f, -0.009f, 0.29f);
                    pc.locked = true;
                    pc.constraintActive = true;
                }

                RotationConstraint rc = weapon.GetComponent<RotationConstraint>();
                if (rc)
                {
                    var constraintSource = new ConstraintSource()
                    {
                        sourceTransform = playerBehaviour.transform,
                        weight = 1
                    };
                    rc.AddSource(constraintSource);
                    // Modify the constraint settings
                    rc.locked = false;
                    // rc.rotationOffset = new Vector3(0, -12.6f, 0);
                    rc.locked = true;
                    rc.constraintActive = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("No PositionConstraint found on weapon: " + weapon.weaponType + " - " + e.Message);
            }

            if (playerBehaviour._weapons.Count > 0)
            {
                playerBehaviour._currentWeapon = playerBehaviour._weapons[playerBehaviour._currentWeaponIndex]
                    .GetComponent<Weapon>();
            }
            else
            {
                Debug.LogError("No weapons found");
            }

            if (playerBehaviour._currentWeapon != null)
            {
                SetClientWeaponActiveServerRpc(playerBehaviour._currentWeapon.NetworkObjectId, false);
            }

            try
            {
                playerBehaviour._currentWeapon = playerBehaviour._weapons[playerBehaviour._currentWeaponIndex]
                    .GetComponent<Weapon>();
                SetClientWeaponActiveServerRpc(playerBehaviour._currentWeapon.NetworkObjectId, true);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning("No weapon found at index: " + playerBehaviour._currentWeaponIndex + " - " +
                                 e.Message);
            }
        }

        #endregion

        #region Destructor

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            _damageReceiver.DamageReceived -= OnDamageReceived;

            if (IsOwner)
            {
                _networkLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;
            }
        }

        public override void OnDestroy()
        {
            Debug.Log("PlayerBehaviour OnDestroy");
            base.OnDestroy();
        }

        #endregion
    }
}