using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Model;
using Unity.Netcode;
using Unity.VisualScripting;
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

        #endregion

        #region InitData

        private void OnEnable()
        {
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
        
        #endregion

        #region Loop

        private void Update()
        {
            if (!GameManager.Instance.isGameStarted.Value) return;
            UpdateWeaponRotation();
            Shoot();
        }

        #endregion
        
        #region Logic

        public void UpdateWeaponRotation()
        {
            Debug.Log("UpdateWeaponRotation -> " + _currentWeapon);
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
        private void SendClientInitDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Behaviour init data ------------------");
            //NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerBehaviour>();
            Debug.Log("Client Id -> " + clientId + " - " + NetworkManager.Singleton.LocalClientId + " - " + IsOwner + " - " + IsLocalPlayer);
            PlayerBehaviour playerBehaviour = NetworkManager.LocalClient.PlayerObject.GetComponent<PlayerBehaviour>();
            playerBehaviour._playerInputController = playerBehaviour.GetComponent<PlayerInputController>();
            playerBehaviour._playerController = playerBehaviour.GetComponent<PlayerController>();
            if (playerBehaviour._defaultWeapon != null)
            {
                EquipWeapon(playerBehaviour._defaultWeapon.weaponType);
            }
            else
            {
                EquipWeapon(WeaponType.Ak47);
            }
            Debug.Log("Player Behaviour READY SendClientInitDataClientRpc -> " + clientId + " " + NetworkManager.Singleton.LocalClientId + " " + IsOwner);
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

        [ServerRpc (RequireOwnership = false)]
        void EquipWeaponServerRpc(int weaponTypeReference, ulong clientId, ServerRpcParams serverRpcParams = default)
        {
            WeaponType weaponType = (WeaponType) weaponTypeReference;
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
                //no.transform.SetParent(RoundManager.Instance.WeaponPool.transform);
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
            //SetWeaponActive();
            
            if (playerBehaviour._currentWeapon != null)
            {
                SetClientWeaponActiveServerRpc(playerBehaviour._currentWeapon.NetworkObjectId, false);
            }
            try
            {
                playerBehaviour._currentWeapon = playerBehaviour._weapons[playerBehaviour._currentWeaponIndex].GetComponent<Weapon>();
                SetClientWeaponActiveServerRpc(playerBehaviour._currentWeapon.NetworkObjectId, true);
                //_currentWeapon.gameObject.SetActive(true);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning("No weapon found at index: " + playerBehaviour._currentWeaponIndex + " - " + e.Message);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ServerRpcParams serverRpcParams = default)
        {
            GameObject impact = Instantiate(_currentWeapon.hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            NetworkObject no = impact.GetComponent<NetworkObject>();
            no.Spawn();
            // Despawn projectile after 2 seconds
            StartCoroutine(DestroyProjectile(no.NetworkObjectId, 2f));
            ShootParticleClientRpc(hitPoint, hitNormal);
        }
        
        private IEnumerator DestroyProjectile(ulong networkObjectId, float timeToDestroy)
        {
            Debug.Log("DestroyProjectile -> " + networkObjectId);
            yield return new WaitForSeconds(timeToDestroy);
            DestroyProjectileServerRpc(networkObjectId);
        }
        
        [ClientRpc]
        void ShootParticleClientRpc (Vector3 hitPoint, Vector3 hitNormal, ClientRpcParams clientRpcParams = default)
        {
            ParticleSystem particleSystem = _currentWeapon.hitEffect.GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();
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