using System.Collections.Generic;
using Model;
using Model.Weapon;
using Model.Weapon.SO;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapons
{
    public class Weapon : NetworkBehaviour
    {
        #region Inspector Variables

        public WeaponType weaponType;
        [SerializeField] private float _damage = 40f;
        [SerializeField] private float _range = 100f; // Range of the weapon
        [SerializeField] private float _fireRate = 1f; // Cadency
        [SerializeField] private float _reloadTime = 1f;
        [SerializeField] private List<AmmoType> _ammoType;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private bool _isReloading = false;
        [SerializeField] private GameObject _hitEffect;

        #endregion

        #region Auxiliary Variables

        [HideInInspector]
        public PlayerBehaviour playerBehaviour;

        private Ammo _currentAmmo;
        bool _canShoot = true;
        float _timerToShoot;

        #endregion

        #region InitData

        private void OnEnable()
        {
            _isReloading = false;
            _canShoot = true;
        }

        private void Start()
        {
            _timerToShoot = _fireRate;
            GetReferences();
        }

        void GetReferences()
        {
            if (_ammoType == null)
            {
                _ammoType = new List<AmmoType>();
                Debug.LogWarning("No ammo type found for " + weaponType + " weapon");
            }
            else if (_currentAmmo == null)
            {
                AmmoSO ammoSO = Resources.Load<AmmoSO>("Weapon/Ammo/" + _ammoType[0].ToString());
                if (ammoSO != null)
                {
                    _currentAmmo = new Ammo(ammoSO);
                }
                else
                {
                    Debug.LogWarning("No AmmoSO found for " + _ammoType[0].ToString() + " ammo type");
                }
            }

            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
                if (_audioSource == null)
                {
                    Debug.LogWarning("No audio source found for " + weaponType + " weapon");
                }
            }

        }

        #endregion

        #region Loop

        private void Update()
        {
            if (_timerToShoot > 0.0f)
            {
                _timerToShoot -= Time.deltaTime;
            }
            else
            {
                _canShoot = true;
            }
        }

        #endregion

        #region Logic
        
        public void Reload()
        {
            if (_isReloading) return;
            _isReloading = true;
            Debug.Log("Reloading...");
            //Invoke("ReloadFinished", _reloadTime);
        }

        public void Shoot()
        {
            if (_canShoot)
            {
                if (_currentAmmo != null && _currentAmmo.IsAmmoInClip())
                {
                    _timerToShoot = _fireRate;
                    _canShoot = false;
                    _currentAmmo.ReduceCurrentAmmo();
                    ShootClientRpc();
                    PlayMuzzleFlash();
                    //TODO: Make projectiles
                    //TEMPORAL
                    ShootProjectileServerRpc();
                    ProcessRaycast();
                }
                else
                {
                    //TODO: reload and play sound
                    Debug.LogWarning("No ammo");
                }
            }
        }

        private void PlayMuzzleFlash()
        {
            if (muzzleFlash != null)
            {
                if (muzzleFlash.isPlaying) muzzleFlash.Stop();
                muzzleFlash.Play();
            }
            else
            {
                Debug.LogWarning("No muzzle flash found");
            }
        }

        private void ProcessRaycast()
        {
            RaycastHit hit;
            Transform cameraTransform = playerBehaviour.PlayerController.PlayerFpsCamera.transform;
            Debug.Log("ProcessRaycast" + cameraTransform.position + " " + cameraTransform.forward);
            ShootServerRpc(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity)) //_range
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _range, Color.green, 1f);
                Debug.Log("Hit");
                string hitTag = hit.transform.gameObject.tag;
                switch (hitTag)
                {
                    case "PLayer":
                        Debug.Log("Player hit");
                        // EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();
                        // if (target == null) return;
                        // target.TakeDamage(_damage); + ammoDamage
                        break;
                    default:
                        Debug.Log("Other hit");
                        CreateHitImpact(hit);
                        break;
                }
            }
            else
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _range, Color.red, 1f);
                Debug.Log("No hit");
                return;
            }
        }

        private void CreateHitImpact(RaycastHit hit)
        {
            if (_hitEffect != null)
            {
                Debug.LogWarning("Hit Effect");
                ShootServerRpc(hit.point, hit.normal);
            }
            else
            {
                Debug.LogWarning("No hit effect found");
            }
        }

        #endregion

        #region Events


        #endregion

        #region Network Calls/Events

        [ClientRpc]
        public void ShootClientRpc()
        {
            if (_audioSource != null)
            {
                if (_audioSource.isPlaying) _audioSource.Stop();
                _audioSource.Play();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("ShootServerRpc -> " + hitPoint + " " + hitNormal);
            GameObject impact = Instantiate(_hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            NetworkObject no = impact.GetComponent<NetworkObject>();
            no.Spawn();
            ParticleSystem particleSystem = impact.GetComponentInChildren<ParticleSystem>();
            particleSystem.Play();
            Destroy(impact, particleSystem.main.duration);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ShootProjectileServerRpc(ServerRpcParams serverRpcParams = default)
        {
            GameObject go = Instantiate(_currentAmmo.GetAmmoPrefab(), transform.position, Quaternion.identity);
            go.GetComponent<ProjectileController>().parent = this;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * 15f;//_currentAmmo.GetShootForce();
            go.GetComponent<NetworkObject>().Spawn(true);
        }

        [ServerRpc(RequireOwnership = false)]
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


        #region Debug

        #endregion
    }
}