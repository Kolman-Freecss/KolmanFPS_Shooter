#region

using System.Collections.Generic;
using Entities.Weapon.SO;
using Model;
using UnityEngine;

#endregion

namespace Entities.Weapon
{
    public class WeaponEntity
    {
        #region Variables - General

        WeaponType weaponType;

        #endregion

        #region Variables - Features

        int damageToArmor;
        int damageToTorso;
        int damageToLegs;
        int damageToArms;
        int damageToHead;
        float range;
        float fireRate;
        float reloadTime;

        #endregion

        #region Variables - Components

        GameObject weaponPrefab;
        List<AmmoType> ammoTypes;
        AudioClip audioSourceHit;
        AudioClip audioSourceShoot;
        AudioClip audioSourceReload;
        AudioClip audioSourceEmpty;

        #endregion

        public WeaponEntity(WeaponSO weaponSO)
        {
            weaponType = weaponSO.WeaponTypeValue;
            damageToArmor = weaponSO.DamageToArmorValue;
            damageToTorso = weaponSO.DamageToTorsoValue;
            damageToLegs = weaponSO.DamageToLegsValue;
            damageToArms = weaponSO.DamageToArmsValue;
            damageToHead = weaponSO.DamageToHeadValue;
            range = weaponSO.RangeValue;
            fireRate = weaponSO.FireRateValue;
            reloadTime = weaponSO.ReloadTimeValue;
            weaponPrefab = weaponSO.WeaponPrefabValue;
            ammoTypes = weaponSO.AmmoTypesValue;
            audioSourceHit = weaponSO.AudioClipHitValue;
            audioSourceShoot = weaponSO.AudioClipShootValue;
            audioSourceReload = weaponSO.AudioClipReloadValue;
            audioSourceEmpty = weaponSO.AudioClipEmptyValue;
        }

        /// <summary>
        /// Returns total damage to player body part
        /// </summary>
        /// <param name="playerBodyPart"></param>
        /// <returns></returns>
        /// //TODO: Add damage to armor, reduce damage taking into account the range, etc.
        public int GetTotalDamage(Player.Player.PlayerBodyPart playerBodyPart, int currentAmmoDamage)
        {
            int totalDamage = 0;
            switch (playerBodyPart)
            {
                case Player.Player.PlayerBodyPart.Arm:
                    totalDamage = damageToArms;
                    break;
                case Player.Player.PlayerBodyPart.Head:
                    totalDamage = damageToHead;
                    break;
                case Player.Player.PlayerBodyPart.Leg:
                    totalDamage = damageToLegs;
                    break;
                case Player.Player.PlayerBodyPart.Torso:
                    totalDamage = damageToTorso;
                    break;
                default:
                    return 0;
            }

            totalDamage += currentAmmoDamage;
            return totalDamage;
        }

        #region Getters

        public WeaponType WeaponTypeValue
        {
            get => weaponType;
        }

        public int DamageToArmorValue
        {
            get => damageToArmor;
        }

        public int DamageToTorsoValue
        {
            get => damageToTorso;
        }

        public int DamageToLegsValue
        {
            get => damageToLegs;
        }

        public int DamageToArmsValue
        {
            get => damageToArms;
        }

        public int DamageToHeadValue
        {
            get => damageToHead;
        }

        public float RangeValue
        {
            get => range;
        }

        public float FireRateValue
        {
            get => fireRate;
        }

        public float ReloadTimeValue
        {
            get => reloadTime;
        }

        public GameObject WeaponPrefabValue
        {
            get => weaponPrefab;
        }

        public List<AmmoType> AmmoTypesValue
        {
            get => ammoTypes;
        }

        public AudioClip AudioClipHitValue
        {
            get => audioSourceHit;
        }

        public AudioClip AudioClipShootValue
        {
            get => audioSourceShoot;
        }

        public AudioClip AudioClipReloadValue
        {
            get => audioSourceReload;
        }

        public AudioClip AudioClipEmptyValue
        {
            get => audioSourceEmpty;
        }

        #endregion
    }
}