#region

using System;
using UnityEngine;

#endregion

namespace Modules.CacheModule
{
    public class PlayerCache : ICacheableEntity<PlayerCache.PlayerCacheKeys>
    {
        public enum PlayerCacheKeys
        {
            Username = 1,
            TeamType = 2,
            MasterVolume = 3,
            MusicVolume = 4,
            ClientGUID = 5
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

        /// <summary>
        /// Returns the value of the key based on the type of the key
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TData GetData<TData>(PlayerCacheKeys key)
        {
            string dataAsString = PlayerPrefs.GetString(key.ToString());
            return (TData)Convert.ChangeType(dataAsString, typeof(TData));
        }

        #endregion
    }
}