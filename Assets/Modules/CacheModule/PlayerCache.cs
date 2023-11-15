using UnityEngine;

namespace Modules.CacheModule
{
    public class PlayerCache : ICacheableEntity<PlayerCache.PlayerCacheKeys>
    {
        
        public enum PlayerCacheKeys
        {
            Username = 1,
            TeamType = 2
        }
        
        public PlayerCache()
        {
        }

        #region Logic

        public void SaveData(PlayerCacheKeys key, string value)
        {
            switch (key)
            {
                default:
                    PlayerPrefs.SetString(key.ToString(), value);
                    break;
            }
        }
        
        public string GetData(PlayerCacheKeys key)
        {
            return PlayerPrefs.GetString(key.ToString());
        }

        #endregion
        
    }
}