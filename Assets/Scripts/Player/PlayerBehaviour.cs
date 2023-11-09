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
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            RoundManager.OnRoundManagerSpawned += InitRound;
        }

        private void InitRound()
        {
            if (IsServer)
            {
                if (_defaultWeapon != null)
                {
                    EquipWeapon(_defaultWeapon.weaponType);
                }
                else
                {
                    EquipWeapon(WeaponType.Ak47);
                }
            }
            if (_weapons.Count > 0)
            {
                _currentWeapon = _weapons[_currentWeaponIndex];
            }
            else
            {
                Debug.LogError("No weapons found");
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
            _health = _maxHealth;
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
            try
            {
                _currentWeapon = _weapons[_currentWeaponIndex];
                _currentWeapon.gameObject.SetActive(true);
            }
            catch (ArgumentOutOfRangeException e)
            {
                Debug.LogWarning("No weapon found at index: " + _currentWeaponIndex + " - " + e.Message);
            }
        }

        void EquipWeapon(WeaponType weaponType)
        {
            EquipWeaponServerRpc((int) weaponType);
        }
        
        #endregion

        #region Network Calls/Events

        [ServerRpc]
        void EquipWeaponServerRpc(int weaponTypeReference, ServerRpcParams serverRpcParams = default)
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
                weaponInstance.GetComponent<Weapon>().playerBehaviour = this;
                NetworkObject no = weaponInstance.GetComponent<NetworkObject>();
                no.Spawn(true);
                try
                {
                    PositionConstraint pc = no.GetComponent<PositionConstraint>();
                    if (pc)
                    {
                        var constraintSource = new ConstraintSource()
                        {
                            sourceTransform = _playerController.playerWeaponCenter,
                            weight = 1
                        };
                        pc.AddSource(constraintSource);
                        pc.constraintActive = true;
                    }
                } catch (Exception e)
                {
                    Debug.LogWarning("No PositionConstraint found on weapon: " + weaponType + " - " + e.Message);
                }
                
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