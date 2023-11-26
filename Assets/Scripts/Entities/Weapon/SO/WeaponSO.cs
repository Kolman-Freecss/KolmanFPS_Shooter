#region

using System.Collections.Generic;
using System.ComponentModel;
using Model;
using UnityEngine;

#endregion

namespace Entities.Weapon.SO
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon/Weapon")]
    public class WeaponSO : ScriptableObject
    {
        #region General

        [Header("General")] [Description("Weapon Type")] [SerializeField]
        WeaponType weaponType;

        public WeaponType WeaponTypeValue
        {
            get => weaponType;
        }

        #endregion

        #region Features

        [Header("Features")] [Description("Weapon damage to armor")] [SerializeField]
        int damageToArmor;

        public int DamageToArmorValue
        {
            get => damageToArmor;
        }

        [Description("Weapon damage to torso")] [SerializeField]
        int damageToTorso;

        public int DamageToTorsoValue
        {
            get => damageToTorso;
        }

        [Description("Weapon damage to legs")] [SerializeField]
        int damageToLegs;

        public int DamageToLegsValue
        {
            get => damageToLegs;
        }

        [Description("Weapon damage to arms")] [SerializeField]
        int damageToArms;

        public int DamageToArmsValue
        {
            get => damageToArms;
        }

        [Description("Weapon damage to head")] [SerializeField]
        int damageToHead;

        public int DamageToHeadValue
        {
            get => damageToHead;
        }

        [Description("Weapon Range")] [SerializeField]
        float range;

        public float RangeValue
        {
            get => range;
        }

        [Description("Weapon Fire Rate")] [SerializeField]
        float fireRate;

        public float FireRateValue
        {
            get => fireRate;
        }

        [Description("Weapon Reload Time")] [SerializeField]
        float reloadTime;

        public float ReloadTimeValue
        {
            get => reloadTime;
        }

        #endregion

        #region Components

        [Header("Components")] [Description("Weapon prefab")] [SerializeField]
        GameObject weaponPrefab;

        public GameObject WeaponPrefabValue
        {
            get => weaponPrefab;
        }

        [Description("Weapon ammo types")] [SerializeField]
        List<AmmoType> ammoTypes;

        public List<AmmoType> AmmoTypesValue
        {
            get => ammoTypes;
        }

        [Description("Weapon audio source hit")] [SerializeField]
        AudioClip audioClipHit;

        public AudioClip AudioClipHitValue
        {
            get => audioClipHit;
        }

        [Description("Weapon audio source shoot")] [SerializeField]
        AudioClip audioClipShoot;

        public AudioClip AudioClipShootValue
        {
            get => audioClipShoot;
        }

        [Description("Weapon audio source reload")] [SerializeField]
        AudioClip audioClipReload;

        public AudioClip AudioClipReloadValue
        {
            get => audioClipReload;
        }

        [Description("Weapon audio source empty")] [SerializeField]
        AudioClip audioClipEmpty;

        public AudioClip AudioClipEmptyValue
        {
            get => audioClipEmpty;
        }

        #endregion
    }
}