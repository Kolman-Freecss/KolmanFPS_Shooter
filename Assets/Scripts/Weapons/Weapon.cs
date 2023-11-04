using System.Collections;
using Model;
using Model.Weapon;
using Player;
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
        [SerializeField] private int _maxAmmo = 10;
        [SerializeField] private Ammo _currentAmmo;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private bool _isReloading = false;
        [SerializeField] private GameObject _hitEffect;

        #endregion

        #region Auxiliary Variables

        PlayerBehaviour _playerBehaviour;

        bool _canShoot = true;

        #endregion

        #region InitData
        
        private void Start()
        {
            GetReferences();
            
        }
        
        void GetReferences()
        {
            if (_currentAmmo == null) _currentAmmo = new Ammo();
            _playerBehaviour = GetComponentInParent<PlayerBehaviour>();
        }
        
        private void OnEnable()
        {
            _isReloading = false;
            _canShoot = true;
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
            if (Physics.Raycast(_playerBehaviour.PlayerController.MainCamera.transform.position, _playerBehaviour.PlayerController.MainCamera.transform.forward, out hit, _range))
            {
                CreateHitImpact(hit);
                // EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();
                // if (target == null) return;
                // target.TakeDamage(_damage);
            }
            else
            {
                return;
            }
        }
        
        private void CreateHitImpact(RaycastHit hit)
        {
            if (_hitEffect != null)
            {
                GameObject impact = Instantiate(_hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impact, 0.1f);
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
            if (_currentAmmo.IsAmmoInClip())
            {
                PlayMuzzleFlash();
                ProcessRaycast();
                _currentAmmo.ReduceCurrentAmmo();
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
        
    }
}