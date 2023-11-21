#region

using Entities.Camera;
using Entities.Player.Skin;
using Gameplay.Player;
using UnityEngine;

#endregion

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

        public enum TeamType
        {
            None = 0,
            Warriors = 1,
            Wizards = 2
        }

        #region Member Variables

        private string m_name;

        public string NameValue
        {
            get => m_name;
            set => m_name = value;
        }

        private TeamType m_teamType;

        public TeamType TeamTypeValue
        {
            get => m_teamType;
            set => m_teamType = value;
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