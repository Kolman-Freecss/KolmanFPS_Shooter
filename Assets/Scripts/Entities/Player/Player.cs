using Entities.Camera;
using Entities.Player.Skin;
using Gameplay.Player;
using UnityEngine;

namespace Entities.Player
{
    public class Player
    {
        
        public enum PlayerTypeSkin
        {
            DefaultSkin = 1,
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

        private GameObject m_playerPrefab;
        
        public GameObject PlayerPrefabValue
        {
            get => m_playerPrefab;
            set => m_playerPrefab = value;
        }

        private PlayerSkin m_PlayerSkin;
        
        public PlayerSkin PlayerSkinValue
        {
            set => m_PlayerSkin = value;
        }
        
        #endregion
        
        public Player()
        {
        }

        #region Getter & Setter
        
        public void Init(CameraMode cameraMode)
        {
            m_PlayerSkin.Init(cameraMode);
        }
        
        public void ChangeCurrentSkinView(CameraMode cameraMode, out SkinView skinView)
        {
            skinView = m_PlayerSkin.ChangeSkinViewByCameraMode(cameraMode);
        }
        
        public TPSPlayerController GetTPSPlayerController()
        {
            return m_PlayerSkin.GetSkinViewByCameraMode(CameraMode.TPS).SkinModel.GetComponent<TPSPlayerController>();
        }
        
        public CameraMode CurrentCameraMode => m_PlayerSkin.CurrentCameraModeValue;
        public GameObject CurrentSkinModel => m_PlayerSkin.CurrentSkinViewValue.SkinModel;
        public Transform RightHand => m_PlayerSkin.CurrentSkinViewValue.SkinParts.RightHand;
        public Transform LeftHand => m_PlayerSkin.CurrentSkinViewValue.SkinParts.LeftHand;
        public Transform Head => m_PlayerSkin.CurrentSkinViewValue.SkinParts.Head;

        #endregion
        
    }
}