﻿#region

using System;
using Entities.Weapon.SO;
using UnityEngine;

#endregion

namespace Entities.Weapon
{
    public class Ammo
    {
        AmmoType ammoType;
        int ammoDamage;
        [HideInInspector] public int AmmoDamage => ammoDamage;
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
            // BasePlayer ammo count
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

        public bool canReload()
        {
            return IsAmmoClips() && !IsAmmoFull();
        }

        public void Reload()
        {
            int ammoNeeded = ammoInClipCapacity - ammoCount;

            ReduceCurrentAmmoClip();
            IncreaseCurrentAmmo(ammoNeeded);
        }

        public String getAmmoInfo()
        {
            return ammoCount + "/" + ammoClips;
        }
    }
}