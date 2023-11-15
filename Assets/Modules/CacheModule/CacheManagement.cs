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
        
        public string GetPlayerCache(PlayerCache.PlayerCacheKeys key)
        {
            return m_playerCache.GetData(key);
        }
        
    }
}