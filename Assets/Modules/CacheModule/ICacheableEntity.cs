namespace Modules.CacheModule
{
    public interface ICacheableEntity <T>
    {
        public void SaveData(T key, string value);
        
        public string GetData(T key);
    }
}