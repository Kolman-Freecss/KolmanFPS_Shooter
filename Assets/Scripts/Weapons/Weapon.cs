﻿using System.Collections;
using System.Collections.Generic;
using Model;
using Model.Weapon;
using Model.Weapon.SO;
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

        private void PlayMuzzleFlashAndAudio()
        {
            if (_audioSource != null)
            {
                if (_audioSource.isPlaying) _audioSource.Stop();
                _audioSource.Play();
            }
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
            if (Physics.Raycast(_playerBehaviour.PlayerController.MainCamera.transform.position, _playerBehaviour.PlayerController.MainCamera.transform.forward, out hit, _range))
            {
                CreateHitImpact(hit);
                // EnemyHealth target = hit.transform.GetComponent<EnemyHealth>();
                // if (target == null) return;
                // target.TakeDamage(_damage); + ammoDamage
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
            if (_currentAmmo != null && _currentAmmo.IsAmmoInClip())
            {
                PlayMuzzleFlashAndAudio();
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