using Model;
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
        [SerializeField] private float _fireRate = 15f;
        [SerializeField] private float _reloadTime = 1f;
        [SerializeField] private int _maxAmmo = 10;
        [SerializeField] private int _currentAmmo = 10;
        [SerializeField] private bool _isReloading = false;
        [SerializeField] private GameObject _hitEffect;

        #endregion

        #region Auxiliary Variables

        PlayerBehaviour _playerBehaviour;

        bool _canShoot = true;

        #endregion

        #region InitData

        private void OnEnable()
        {
            _isReloading = false;
            _canShoot = true;
        }

        #endregion

        #region Logic

        private void Update()
        {
            if (_isReloading) return;
            if (_currentAmmo <= 0)
            {
                // StartCoroutine(Reload());
                return;
            }

            if (Input.GetButtonDown("Fire1") && _canShoot)
            {
                // StartCoroutine(Shoot());
            }
        }

        #endregion
    }
}