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
            _currentWeaponIndex = 0;
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

        #endregion
        
    }
}