using System;
using System.Collections.Generic;
using Config;
using Model;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using Weapons;

namespace Player
{
    public class PlayerBehaviour : NetworkBehaviour
    {

        #region Inspector variables

        [Header("Player")]
        [Tooltip("Max health of the player")]
        [SerializeField] private float _maxHealth = 100f;
        
        [Header("Weapons")]
        [Tooltip("Default weapon of the player")]
        [SerializeField] private Weapon _defaultWeapon;

        #endregion

        #region Auxiliary Variables

        PlayerInputController _playerInputController;
        PlayerController _playerController;
        [HideInInspector]
        public PlayerController PlayerController => _playerController;
        List<NetworkObject> _weapons = new List<NetworkObject>();
        Weapon _currentWeapon;
        int _currentWeaponIndex = 0;

        private float _health = 100f;
        private bool _ready = false;

        #endregion

        #region InitData

        private void OnEnable()
        {
            _ready = false;
            _playerController = GetComponent<PlayerController>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                RegisterServerCallbacks();
            }
            Debug.Log("PlayerBehaviour OnNetworkSpawn + " + NetworkObjectId + " " + NetworkManager.Singleton.LocalClientId + " " + IsOwner);
        }
        
        private void RegisterServerCallbacks()
        {
            RoundManager.OnRoundManagerSpawned += InitRound;
        }

        private void InitRound()
        {
            SceneTransitionHandler.Instance.OnClientLoadedGameScene += ClientLoadedGameScene;
            Debug.Log("InitRound -> " + NetworkObjectId + " " + NetworkManager.Singleton.LocalClientId + " " + IsOwner);
        }

        /// <summary>
        /// Invoked when the object is instantiated, we need to wait for the scene to load
        /// </summary>
        private void Start()
        {
            Init();
        }
        
        void Init()
        {
            _currentWeaponIndex = 0;
            _health = _maxHealth;
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
                        TargetClientIds = new ulong[] {clientId}
                    }
                };
                SendClientInitDataClientRpc(clientId, clientRpcParams);
            }
        }
        
        [ClientRpc]
        private void SendClientInitDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Behaviour init data ------------------");
            //NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerBehaviour>();
            Debug.Log("Client Id -> " + clientId + " - " + NetworkManager.Singleton.LocalClientId + " - " + IsOwner + " - " + IsLocalPlayer);
            _playerInputController = GetComponent<PlayerInputController>();
            _playerController = GetComponent<PlayerController>();
            if (_defaultWeapon != null)
            {
                EquipWeapon(_defaultWeapon.weaponType);
            }
            else
            {
                EquipWeapon(WeaponType.Ak47);
            }
            _ready = true;
        }

        #endregion

        #region Loop

        private void Update()
        {
            if (!GameManager.Instance.isGameStarted.Value ||!_ready) return;
            UpdateWeaponRotation();
            Shoot();
        }

        #endregion
        
        #region Logic

        public void UpdateWeaponRotation()
        {
            if (_currentWeapon == null) return;
            Vector3 desiredRotation = _playerController.MainCamera.transform.localRotation.eulerAngles;
            _currentWeapon.transform.localRotation = Quaternion.Euler(desiredRotation.x, desiredRotation.y, 0f);
            // Move weapon Y and Z position in proportion to camera X rotation
            // float proportionFactor = 2f;
            // _currentWeapon.transform.localPosition = new Vector3(
            //     _currentWeapon.transform.localPosition.x + desiredRotation.x * proportionFactor,
            //     _currentWeapon.transform.localPosition.y + desiredRotation.x * proportionFactor,
            //     _currentWeapon.transform.localPosition.z + desiredRotation.x * proportionFactor
            // );
        }

        void Shoot()
        {
            if (_playerInputController.leftClick)
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
                        ShootClientRpc();
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
            ShootServerRpc(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity)) //range
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _currentWeapon.range, Color.green, 1f);
                Debug.Log("Hit");
                string hitTag = hit.transform.gameObject.tag;
                switch (hitTag)
                {
                    case "PLayer":
                        Debug.Log("Player hit");
                        // EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();
                        // if (target == null) return;
                        // target.TakeDamage(damage); + ammoDamage
                        break;
                    default:
                        Debug.Log("Other hit");
                        CreateHitImpact(hit);
                        break;
                }
            }
            else
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _currentWeapon.range, Color.red, 1f);
                Debug.Log("No hit");
                return;
            }
        }
        
        private void CreateHitImpact(RaycastHit hit)
        {
            if (_currentWeapon.hitEffect != null)
            {
                Debug.LogWarning("Hit Effect");
                ShootServerRpc(hit.point, hit.normal);
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
        
        void SetWeaponActive()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.gameObject.SetActive(false);
            }
            try
            {
                _currentWeapon = _weapons[_currentWeaponIndex].GetComponent<Weapon>();
                _currentWeapon.gameObject.SetActive(true);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning("No weapon found at index: " + _currentWeaponIndex + " - " + e.Message);
            }
        }

        /// <summary>
        /// We call server to spawn the weapon and then we equip it
        /// </summary>
        /// <param name="weaponType"></param>
        void EquipWeapon(WeaponType weaponType)
        {
            EquipWeaponServerRpc((int) weaponType, NetworkManager.Singleton.LocalClientId);
        }
        
        #endregion

        #region Network Calls/Events
        
        [ClientRpc]
        public void ShootClientRpc()
        {
            if (_currentWeapon.audioSource != null)
            {
                if (_currentWeapon.audioSource.isPlaying) _currentWeapon.audioSource.Stop();
                _currentWeapon.audioSource.Play();
            }
        }

        [ServerRpc (RequireOwnership = false)]
        void EquipWeaponServerRpc(int weaponTypeReference, ulong clientId, ServerRpcParams serverRpcParams = default)
        {
            WeaponType weaponType = (WeaponType) weaponTypeReference;
            String path = "Weapon/" + weaponType.ToString();
            GameObject weaponPrefab = Resources.Load<GameObject>(path);
            if (weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab, RoundManager.Instance.WeaponPool.transform);
                weaponInstance.transform.localPosition = weaponPrefab.transform.position;
                weaponInstance.transform.localRotation = Quaternion.identity;
                weaponInstance.transform.localScale = weaponPrefab.transform.localScale;
                weaponInstance.SetActive(false);
                NetworkObject no = weaponInstance.GetComponent<NetworkObject>();
                no.Spawn();//SpawnWithOwnership(clientId);
                Debug.Log("EquipWeaponServerRpc -> " + clientId + " " + no.NetworkObjectId);
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] {clientId}
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
        private void AttachSpawnedWeaponClientRpc(ulong clientId, ulong networkObjectId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("AddNewWeaponClientRpc -> " + clientId + " " + networkObjectId);
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
                    var constraintSource = new ConstraintSource()
                    {
                        sourceTransform = playerBehaviour._playerController.playerWeaponCenter,
                        weight = 1
                    };
                    pc.AddSource(constraintSource);
                    pc.constraintActive = true;
                }
            } catch (Exception e)
            {
                Debug.LogWarning("No PositionConstraint found on weapon: " + weapon.weaponType + " - " + e.Message);
            }
            
            if (playerBehaviour._weapons.Count > 0)
            {
                playerBehaviour._currentWeapon = playerBehaviour._weapons[playerBehaviour._currentWeaponIndex].GetComponent<Weapon>();
            }
            else
            {
                Debug.LogError("No weapons found");
            }
            SetWeaponActive();
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ServerRpcParams serverRpcParams = default)
        {
            GameObject impact = Instantiate(_currentWeapon.hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            NetworkObject no = impact.GetComponent<NetworkObject>();
            no.Spawn();
            ParticleSystem particleSystem = impact.GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();
            Destroy(impact, particleSystem.main.duration);
        }

        [ServerRpc]
        public void ShootProjectileServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GameObject go = Instantiate(_currentWeapon.currentAmmo.GetAmmoPrefab(), transform.position, Quaternion.identity);
            go.GetComponent<ProjectileController>().parent = this;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * 15f;//currentAmmo.GetShootForce();
            go.GetComponent<NetworkObject>().Spawn(true);
        }

        [ServerRpc]
        public void DestroyProjectileServerRpc(ulong networkObjectId, ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("DestroyProjectileServerRpc -> " + networkObjectId);
            NetworkObject no = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
            if (no != null)
            {
                no.Despawn();
                Destroy(no.gameObject);
            }
        }

        #endregion
        
    }
}