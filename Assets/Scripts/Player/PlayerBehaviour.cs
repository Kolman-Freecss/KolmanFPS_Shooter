using System;
using System.Collections.Generic;
using Config;
using Model;
using UnityEngine;
using Weapons;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerBehaviour))]
    public class PlayerBehaviour : MonoBehaviour
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
        List<Weapon> _weapons = new List<Weapon>();
        Weapon _currentWeapon;
        int _currentWeaponIndex = 0;

        private float _health = 100f;

        #endregion

        #region InitData

        private void Awake()
        {
            GetReferences();
        }
        
        void GetReferences()
        {
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
            SetWeaponActive();
        }

        private void Start()
        {
            Init();
        }
        
        void Init()
        {
            _currentWeaponIndex = 0;
            if (_weapons.Count > 0)
            {
                _currentWeapon = _weapons[_currentWeaponIndex];
            }
            else
            {
                Debug.LogError("No weapons found");
            }
            _health = _maxHealth;
        }

        #endregion

        #region Loop

        private void Update()
        {
            if (!GameManager.Instance.isGameStarted.Value) return; 
            Shoot();
        }

        #endregion
        
        #region Logic

        void Shoot()
        {
            if (_playerInputController.leftClick)
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
        
        void SetWeaponActive()
        {
            if (_currentWeapon != null)
            {
                _currentWeapon.gameObject.SetActive(false);
            }
            _currentWeapon = _weapons[_currentWeaponIndex];
            _currentWeapon.gameObject.SetActive(true);
        }

        void EquipWeapon(WeaponType weaponType)
        {
            String path = "Weapon/" + weaponType.ToString();
            GameObject weaponPrefab = Resources.Load<GameObject>(path);
            
            if (weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab, transform);
                weaponInstance.transform.localPosition = weaponPrefab.transform.position;
                weaponInstance.transform.localRotation = Quaternion.identity;
                weaponInstance.transform.localScale = weaponPrefab.transform.localScale;
                weaponInstance.SetActive(false);
                _weapons.Add(weaponInstance.GetComponent<Weapon>());
            }
            else
            {
                Debug.LogError("Weapon prefab not found at path: " + path);
            }
            
        }

        #endregion
        
    }
}