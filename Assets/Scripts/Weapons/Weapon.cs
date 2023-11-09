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
        public float damage = 40f;
        public float range = 100f; // Range of the weapon
        public float fireRate = 1f; // Cadency
        public float reloadTime = 1f;
        public List<AmmoType> ammoType;
        public ParticleSystem muzzleFlash;
        public AudioSource audioSource;
        public bool isReloading = false;
        public GameObject hitEffect;

        #endregion

        #region Auxiliary Variables

        [HideInInspector]
        public PlayerBehaviour playerBehaviour;

        [HideInInspector]
        public Ammo currentAmmo;
        [HideInInspector]
        public bool canShoot = true;
        [HideInInspector]
        public float timerToShoot;

        #endregion

        #region InitData

        private void OnEnable()
        {
            isReloading = false;
            canShoot = true;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            Debug.Log("Weapon OnNetworkSpawn + " + weaponType + " " + NetworkObjectId + " " + NetworkManager.Singleton.LocalClientId + " " + IsOwner);
        }

        private void Start()
        {
            timerToShoot = fireRate;
            GetReferences();
        }

        void GetReferences()
        {
            if (ammoType == null || ammoType.Count == 0)
            {
                ammoType = new List<AmmoType>();
                Debug.LogWarning("No ammo type found for " + weaponType + " weapon");
            }
            else if (currentAmmo == null)
            {
                AmmoSO ammoSO = Resources.Load<AmmoSO>("Weapon/Ammo/" + ammoType[0].ToString());
                if (ammoSO != null)
                {
                    currentAmmo = new Ammo(ammoSO);
                }
                else
                {
                    Debug.LogWarning("No AmmoSO found for " + ammoType[0].ToString() + " ammo type");
                }
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    Debug.LogWarning("No audio source found for " + weaponType + " weapon");
                }
            }

        }

        #endregion

        #region Loop

        private void Update()
        {
            if (timerToShoot > 0.0f)
            {
                timerToShoot -= Time.deltaTime;
            }
            else
            {
                canShoot = true;
            }
        }

        #endregion

        #region Logic
        
        public void Reload()
        {
            if (isReloading) return;
            isReloading = true;
            Debug.Log("Reloading...");
            //Invoke("ReloadFinished", reloadTime);
        }

        public void PlayMuzzleFlash()
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

        #endregion

        #region Events


        #endregion

        #region Network Calls/Events
        
        

        #endregion


        #region Debug

        #endregion
    }
}