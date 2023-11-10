using System;
using Model.Weapon.SO;
using UnityEngine;

namespace Model.Weapon
{
    public class Ammo
    {
        AmmoType ammoType;
        int ammoDamage;
        int ammoCount;
        int ammoInClipCapacity;
        int ammoClips;
        GameObject ammoPrefab;

        #region Public properties

        public AmmoType AmmoTypeValue
        {
            get => ammoType;
        }
        
        public int AmmoCountValue
        {
            get => ammoCount;
        }
        
        public int AmmoInClipCapacityValue
        {
            get => ammoInClipCapacity;
        }
        
        public int AmmoClipsValue
        {
            get => ammoClips;
        }

        #endregion

        public Ammo()
        {
            ammoCount = 0;
        }
        
        public Ammo(AmmoSO ammoSO) : this()
        {
            ammoType = ammoSO.AmmoTypeValue;
            ammoInClipCapacity = ammoSO.AmmoInClipCapacityValue;
            ammoClips = ammoSO.AmmoClipsValue;
            ammoPrefab = ammoSO.AmmoPrefabValue;
            ammoDamage = ammoSO.AmmoDamageValue;
            // Default ammo count
            ammoCount = ammoInClipCapacity;
        }
        
        public void ReduceCurrentAmmo()
        {
            ammoCount--;
        }
        
        public void IncreaseCurrentAmmo(int ammoAmount)
        {
            ammoCount += ammoAmount;
        }
        
        public void ReduceCurrentAmmoClip()
        {
            ammoClips--;
        }

        public void IncreaseCurrentAmmoClip(int ammoAmount)
        {
            ammoClips += ammoAmount;
        }
        
        public bool IsAmmoInClip()
        {
            return ammoCount > 0;
        }
        
        public bool IsAmmoClips()
        {
            return ammoClips > 0;
        }
        
        public bool IsAmmoFull()
        {
            return ammoCount == ammoInClipCapacity;
        }
        
        public bool IsAmmoClipsFull()
        {
            return ammoClips == ammoInClipCapacity;
        }
        
        public GameObject GetAmmoPrefab()
        {
            return ammoPrefab;
        }
        
        public void Reload()
        {
            if (IsAmmoClips() && !IsAmmoFull())
            {
                int ammoNeeded = ammoInClipCapacity - ammoCount;
                if (ammoNeeded > ammoClips)
                {
                    ammoNeeded = ammoClips;
                }
                ReduceCurrentAmmoClip();
                IncreaseCurrentAmmo(ammoNeeded);
            }
        }
        
        public String getAmmoInfo()
        {
            return ammoCount + "/" + ammoClips;
        } 
            
    }
}