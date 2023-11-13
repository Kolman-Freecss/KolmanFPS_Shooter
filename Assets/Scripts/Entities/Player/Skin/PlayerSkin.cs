using System.Collections.Generic;
using System.ComponentModel;
using Entities.Camera;
using Entities.Utils;
using UnityEngine;

namespace Entities.Player
{
    public class PlayerSkin : MonoBehaviour
    {
        [Description("Skin")]
        [SerializeField]
        Player.PlayerSkin _skin;
        
        public Player.PlayerSkin SkinValue
        {
            get => _skin;
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