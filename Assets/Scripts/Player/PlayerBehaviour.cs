#region

using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Gameplay.GameplayObjects;
using Model;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Weapons;

#endregion

namespace Player
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
            if (IsServer)
            {
                RegisterServerCallbacks();
            }

            _damageReceiver.DamageReceived += OnDamageReceived;

            if (IsOwner)
            {
                _networkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
            }
        }

        private void RegisterServerCallbacks()
        {
            // GameManager.Instance.OnGameStarted += InitRoundPlayers;
            // RoundManager.OnRoundStarted += InitRound;
            // RoundManager.Instance.OnRoundManagerSpawned += InitRound;
        }

        /// <summary>
        /// When the round manager is spawned we need to wait for the scene to load
        /// </summary>
        // private void InitRound()
        // {
        //     RoundManager.OnRoundStarted += InitRoundPlayers;
        //     // SceneTransitionHandler.Instance.OnClientLoadedGameScene += ClientLoadedGameScene;
        //     // GameManager.Instance.OnGameStarted += ClientLoadedGameScene;
        //     Debug.Log("InitRound -> " + NetworkObjectId + " " + NetworkManager.Singleton.LocalClientId + " " + IsOwner);
        // }

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

                if (_currentWeapon.canShoot)
                {
                    if (_currentWeapon.currentAmmo != null && _currentWeapon.currentAmmo.IsAmmoInClip())
                    {
                        _currentWeapon.timerToShoot = _currentWeapon.fireRate;
                        _currentWeapon.canShoot = false;
                        _currentWeapon.currentAmmo.ReduceCurrentAmmo();
                        ShootAudioServerRpc(_currentWeapon.NetworkObjectId);
                        _currentWeapon.PlayMuzzleFlash();
                        //TODO: Make projectiles
                        //TEMPORAL
                        /////ShootProjectileServerRpc();
                        ProcessShootRaycast();
                    }
                    else
                    {
                        //TODO: reload and play sound
                        Debug.LogWarning("No ammo");
                    }
                }
            }
        }

        public void ProcessShootRaycast()
        {
            RaycastHit hit;
            Transform cameraTransform = PlayerController.PlayerFpsCamera.transform;
            // ShootServerRpc(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity)) //range
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _currentWeapon.range, Color.green,
                    1f);
                string hitTag = hit.transform.gameObject.tag;
                switch (hitTag)
                {
                    case "Player":
                        DamageReceiver damageReceiver = hit.transform.gameObject.GetComponent<DamageReceiver>();
                        if (damageReceiver == null) return;
                        damageReceiver.ReceiveDamage(this, _currentWeapon.GetTotalDamage());
                        CreateHitImpact(hit, true);
                        break;
                    default:
                        CreateHitImpact(hit);
                        break;
                }
            }
            else
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _currentWeapon.range, Color.red, 1f);
                Debug.Log("No hit");
            }
        }

        private void CreateHitImpact(RaycastHit hit, bool isPlayer = false)
        {
            if (_currentWeapon.hitEffect != null)
            {
                ShootServerRpc(hit.point, hit.normal, _currentWeapon.NetworkObjectId, isPlayer);
            }
            else
            {
                Debug.LogWarning("No hit effect found");
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
            //Get Components
            PlayerBehaviour playerBehaviour = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerBehaviour>();
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

        /// <summary>
        /// Invoke the audio source from the networkObject client that called the server to the rest of the clients 
        /// </summary>
        [ServerRpc]
        public void ShootAudioServerRpc(ulong networkObjectId)
        {
            ShootAudioClientRpc(NetworkManager.Singleton.LocalClientId, networkObjectId);
        }

        [ClientRpc]
        private void ShootAudioClientRpc(ulong clientId, ulong networkObjectId)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            Weapon sourceWeapon = no.GetComponent<Weapon>();
            if (sourceWeapon.audioSource != null)
            {
                if (sourceWeapon.audioSource.isPlaying) sourceWeapon.audioSource.Stop();
                sourceWeapon.audioSource.Play();
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
            NetworkObject player = NetworkManager.LocalClient.PlayerObject;
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            PlayerBehaviour playerBehaviour = player.GetComponent<PlayerBehaviour>();
            playerBehaviour._weapons.Add(no);
            Weapon weapon = no.GetComponent<Weapon>();
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

        [ServerRpc(RequireOwnership = false)]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ulong networkObjectId, bool isPlayer,
            ServerRpcParams serverRpcParams = default)
        {
            NetworkObject weaponNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            GameObject hitEffect = isPlayer
                ? weaponNetworkObject.GetComponent<Weapon>().playerHitEffect
                : weaponNetworkObject.GetComponent<Weapon>().hitEffect;
            GameObject impact = Instantiate(hitEffect, hitPoint,
                Quaternion.LookRotation(hitNormal));
            NetworkObject no = impact.GetComponent<NetworkObject>();
            no.Spawn();
            // TODO: Despawn projectile after some time
            StartCoroutine(DestroyProjectile(no.NetworkObjectId, 2f));
            ShootParticleClientRpc(hitPoint, hitNormal, no.NetworkObjectId);
        }

        private IEnumerator DestroyProjectile(ulong networkObjectId, float timeToDestroy)
        {
            yield return new WaitForSeconds(timeToDestroy);
            DestroyProjectileServerRpc(networkObjectId);
        }

        [ClientRpc]
        void ShootParticleClientRpc(Vector3 hitPoint, Vector3 hitNormal, ulong networkObjectId,
            ClientRpcParams clientRpcParams = default)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            ParticleSystem particleSystem = no.GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();
        }

        [ServerRpc]
        public void ShootProjectileServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GameObject go = Instantiate(_currentWeapon.currentAmmo.GetAmmoPrefab(), transform.position,
                Quaternion.identity);
            go.GetComponent<ProjectileController>().parent = this;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * 15f; //currentAmmo.GetShootForce();
            go.GetComponent<NetworkObject>().Spawn(true);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DestroyProjectileServerRpc(ulong networkObjectId, ServerRpcParams serverRpcParams = default)
        {
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            if (no != null)
            {
                no.Despawn();
                Destroy(no.gameObject);
            }
        }

        #endregion

        #region Destructor

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsServer)
            {
                // RoundManager.OnRoundStarted -= InitRound;
                //SceneTransitionHandler.Instance.OnClientLoadedGameScene -= ClientLoadedGameScene;
                GameManager.Instance.OnGameStarted -= ClientLoadedGameScene;
            }

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