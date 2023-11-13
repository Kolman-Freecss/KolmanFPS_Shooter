namespace Entities.Utils
{
    /// <summary>
    /// Util class to serialize a dictionary inside a ScriptableObject
    /// <snippet>
    ///     List<SerializableDictionaryEntry<CameraMode, SkinView>> _skinViews;
    /// </snippet>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [System.Serializable]
    public class SerializableDictionaryEntry<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
    }
}