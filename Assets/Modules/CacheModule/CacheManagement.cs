namespace Modules.CacheModule
{
    public class CacheManagement
    {
        public PlayerCache m_playerCache { get; set; }

        public CacheManagement()
        {
            m_playerCache = new PlayerCache();
        }

        public void SavePlayerCache(PlayerCache.PlayerCacheKeys key, string value)
        {
            m_playerCache.SaveData(key, value);
        }

        /// <summary>
        /// Gets the value of the key based on the type of the key
        /// </summary>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public TData GetPlayerCache<TData>(PlayerCache.PlayerCacheKeys key)
        {
            return m_playerCache.GetData<TData>(key);
        }
    }
}