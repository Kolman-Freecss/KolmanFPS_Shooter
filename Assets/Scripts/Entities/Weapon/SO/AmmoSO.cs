using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entities.Weapon.SO
{
    [CreateAssetMenu(fileName = "New Ammo",menuName = "Weapon/Ammo")]
    public class AmmoSO : ScriptableObject
    {
        
        [Description("Ammo type")]
        [FormerlySerializedAs("ammoType")] [SerializeField]
        AmmoType _ammoType;
        
        public AmmoType AmmoTypeValue
        {
            get => _ammoType;
        }
        
        [Description("Ammo in clip capacity")]
        [FormerlySerializedAs("ammoInClipCapacity")] [SerializeField]
        int _ammoInClipCapacity;
        
        public int AmmoInClipCapacityValue
        {
            get => _ammoInClipCapacity;
        }
        
        [Description("Ammo clips")]
        [FormerlySerializedAs("ammoClips")] [SerializeField]
        int _ammoClips;
        
        public int AmmoClipsValue
        {
            get => _ammoClips;
        }
        
        [Description("Ammo prefab")]
        [FormerlySerializedAs("ammoPrefab")] [SerializeField]
        GameObject _ammoPrefab;
        
        public GameObject AmmoPrefabValue
        {
            get => _ammoPrefab;
        }
        
        [Description("Ammo damage")]
        [FormerlySerializedAs("ammoDamage")] [SerializeField]
        int _ammoDamage;
        
        public int AmmoDamageValue
        {
            get => _ammoDamage;
        }
        
    }
}