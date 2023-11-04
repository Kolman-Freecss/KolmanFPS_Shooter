using Model.Weapon.SO;

namespace Model.Weapon
{
    public class Ammo
    {
        AmmoType ammoType;
        int ammoCount;
        int ammoCapacity;
        int ammoInClipCapacity;
        int ammoClips;

        #region Public properties

        public AmmoType AmmoTypeValue
        {
            get => ammoType;
        }
        
        public int AmmoCountValue
        {
            get => ammoCount;
        }
        
        public int AmmoCapacityValue
        {
            get => ammoCapacity;
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

        public Ammo(AmmoSO ammoSO)
        {
            ammoType = ammoSO.AmmoTypeValue;
            ammoCapacity = ammoSO.AmmoCapacityValue;
            ammoInClipCapacity = ammoSO.AmmoInClipCapacityValue;
            ammoClips = ammoSO.AmmoClipsValue;
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
            return ammoClips == ammoCapacity;
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
    }
}