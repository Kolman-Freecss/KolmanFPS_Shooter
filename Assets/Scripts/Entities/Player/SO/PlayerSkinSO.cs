using System.Collections.Generic;
using System.ComponentModel;
using Entities.Camera;
using Entities.Utils;
using UnityEngine;

namespace Entities.Player.SO
{
    [CreateAssetMenu(fileName = "New Skin",menuName = "Player/Skins")]
    public class PlayerSkinSO : ScriptableObject
    {
        [Description("Skin")]
        [SerializeField]
        Player.PlayerSkin _skin;
        
        public Player.PlayerSkin SkinValue
        {
            get => _skin;
        }
        
        [Description("Skin prefab")]
        [SerializeField]
        GameObject _skinPrefab;
        
        public GameObject SkinPrefabValue
        {
            get => _skinPrefab;
        }
        
        [Description("Skin View - Positions of the player's body parts")]
        [SerializeField]
        List<SerializableDictionaryEntry<CameraMode, SkinView>> _skinViews;
        
        public List<SerializableDictionaryEntry<CameraMode, SkinView>> SkinViewsValue
        {
            get => _skinViews;
        }
    }
}