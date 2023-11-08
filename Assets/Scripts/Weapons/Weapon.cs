using System;
using System.Collections;
using System.Collections.Generic;
using Config;
using Model;
using Model.Weapon;
using Model.Weapon.SO;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Weapons
{
    public class Weapon : MonoBehaviour
    {
        #region Inspector Variables

        public WeaponType weaponType;
        [SerializeField] private float _damage = 40f;
        [SerializeField] private float _range = 100f;
        [SerializeField] private float _fireRate = 1f;
        [SerializeField] private float _reloadTime = 1f;
        [SerializeField] private List<AmmoType> _ammoType;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private bool _isReloading = false;
        [SerializeField] private GameObject _hitEffect;

        #endregion

        #region Auxiliary Variables

        PlayerBehaviour _playerBehaviour;

        private Ammo _currentAmmo;
        bool _canShoot = true;

        #endregion

        #region InitData

        private void OnEnable()
        {
            _isReloading = false;
            _canShoot = true;
        }

        private void Start()
        {
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

            _playerBehaviour = GetComponentInParent<PlayerBehaviour>();
        }

        #endregion

        #region Logic

        public void Shoot()
        {
            if (_canShoot)
            {
                StartCoroutine(Shooting());
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
            Transform cameraTransform = _playerBehaviour.PlayerController.MainCamera.transform;
            Debug.Log("ProcessRaycast" + cameraTransform.position + " " + cameraTransform.forward);
            ShootServerRpc(cameraTransform.position, cameraTransform.forward);
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, _range))
            {
                Debug.DrawRay(cameraTransform.position, cameraTransform.forward * _range, Color.green, 1f);
                // Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * _range);
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
                // Gizmos.DrawRay(cameraTransform.position, cameraTransform.forward * _range);
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

        IEnumerator Shooting()
        {
            _canShoot = false;
            if (_currentAmmo != null && _currentAmmo.IsAmmoInClip())
            {
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

            yield return new WaitForSeconds(_fireRate);
            _canShoot = true;
        }

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
        
        [ServerRpc]
        public void ShootServerRpc(Vector3 hitPoint, Vector3 hitNormal, ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("ShootServerRpc -> " + hitPoint + " " + hitNormal);
            // GameObject impact = Instantiate(_hitEffect, hitPoint, Quaternion.LookRotation(hitNormal));
            // Destroy(impact, 0.1f);
            Transform cameraTransform = _playerBehaviour.PlayerController.MainCamera.transform;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, _range))
            {
                Debug.Log("ShootServerRpc -> " + hit.transform.gameObject.tag);
                string hitTag = hit.transform.gameObject.tag;
                switch (hitTag)
                {
                    case "Player":
                        Debug.Log("Player hit");
                        // EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();
                        // if (target == null) return;
                        // target.TakeDamage(_damage); + ammoDamage
                        break;
                    default:
                        Debug.Log("Other hit");
                        // CreateHitImpact(hit);
                        break;
                }
            }
            else
            {
                Debug.Log("ShootServerRpc -> No hit");
            }
        }

        [ServerRpc]
        public void ShootProjectileServerRpc()
        {
            GameObject go = Instantiate(_currentAmmo.GetAmmoPrefab(), transform.position, Quaternion.identity);
            go.GetComponent<MoveProjectile>().parent = this;
            go.GetComponent<Rigidbody>().velocity = go.transform.forward * 15f;//_currentAmmo.GetShootForce();
            go.GetComponent<NetworkObject>().Spawn();
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


        #region Debug

        #endregion
    }
}