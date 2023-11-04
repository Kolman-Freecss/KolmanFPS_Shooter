using System;
using System.Collections.Generic;
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

        #endregion

        #region Auxiliary Variables

        PlayerInputController _playerInputController;
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
            _playerInputController = PlayerInputController.Instance;
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
            Shoot();
        }

        #endregion
        
        #region Logic

        void Shoot()
        {
            if (_playerInputController.leftClick)
            {
                Debug.Log("Shoot");
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
        
        void EquipWeapon(Weapon weapon)
        {
            String path = "Prefabs/Weapons/" + weapon.weaponType.ToString();
            GameObject weaponPrefab = Resources.Load<GameObject>(path);
            
            if (weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab, transform);
                weaponInstance.transform.localPosition = Vector3.zero;
                weaponInstance.transform.localRotation = Quaternion.identity;
                weaponInstance.transform.localScale = Vector3.one;
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