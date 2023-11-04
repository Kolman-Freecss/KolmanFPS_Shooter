using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;

namespace Model.Weapon.SO
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
        
        [Description("Ammo capacity in clip")]
        [FormerlySerializedAs("ammoCapacity")] [SerializeField]
        int _ammoCapacity;
        
        public int AmmoCapacityValue
        {
            get => _ammoCapacity;
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
        
    }
}