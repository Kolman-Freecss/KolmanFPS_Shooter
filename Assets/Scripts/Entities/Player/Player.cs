using Entities.Camera;
using Entities.Player.SO;
using UnityEngine;

namespace Entities.Player
{
    public class Player
    {
        
        public enum PlayerSkin
        {
            BasePlayer = 1,
            Krodun = 2,
            Kolman = 3
        }

        #region Member Variables

        private string m_name;

        public string NameValue
        {
            get => m_name;
            set => m_name = value;
        }

        private SkinView m_currentSkinView;
        
        public SkinView CurrentSkinViewValue
        {
            get => m_currentSkinView;
            set => m_currentSkinView = value;
        }
        
        private PlayerSkinSO m_playerSkinSO;
        
        public PlayerSkinSO PlayerSkinSOValue
        {
            set => m_playerSkinSO = value;
        }
        
        #endregion
        
        public Player()
        {
            m_playerSkinSO = new PlayerSkinSO();
        }
        
        public Player(PlayerSkinSO playerSkinSO)
        {
            m_playerSkinSO = playerSkinSO;
        }

        #region Getter & Setter
        
        public void ChangeCurrentSkinView(CameraMode cameraMode, out SkinView skinView)
        {
            skinView = GetSkinView(cameraMode);
            m_currentSkinView = skinView;
        }
        
        public SkinView GetSkinView(CameraMode cameraMode)
        {
            foreach (var skinView in m_playerSkinSO.SkinViewsValue)
            {
                if (skinView.Key == cameraMode)
                {
                    return skinView.Value;
                }
            }

            return null;
        }
        
        public Transform RightHand => CurrentSkinViewValue.SkinParts.RightHand;
        public Transform LeftHand => CurrentSkinViewValue.SkinParts.LeftHand;
        public Transform Head => CurrentSkinViewValue.SkinParts.Head;

        #endregion
        
    }
}