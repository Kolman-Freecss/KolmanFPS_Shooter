using System;
using System.Collections.Generic;
using System.ComponentModel;
using Entities.Camera;
using Entities.Utils;
using Gameplay.Player;
using UnityEngine;

namespace Entities.Player.Skin
{
    public class PlayerSkin : MonoBehaviour
    {
        #region Inspector Variables

        [Description("Type Skin")] [SerializeField]
        Player.PlayerTypeSkin typeSkin;

        public Player.PlayerTypeSkin TypeSkinValue
        {
            get => typeSkin;
        }

        [Description("Team Skin")] [SerializeField]
        Player.TeamType teamSkin;

        public Player.TeamType TeamSkinValue
        {
            get => teamSkin;
        }

        [Description("typeSkin View - Positions of the player's body parts")] [SerializeField]
        List<SerializableDictionaryEntry<CameraMode, SkinView>> _skinViews;

        public List<SerializableDictionaryEntry<CameraMode, SkinView>> SkinViewsValue
        {
            get => _skinViews;
        }

        #endregion

        #region Member Variables

        private CameraMode m_currentCameraMode;

        public CameraMode CurrentCameraModeValue
        {
            get => m_currentCameraMode;
        }

        private TPSPlayerController m_tpsPlayerController;

        public TPSPlayerController TPSPlayerControllerValue
        {
            get => m_tpsPlayerController;
            set => m_tpsPlayerController = value;
        }

        private SkinView m_currentSkinView;

        public SkinView CurrentSkinViewValue
        {
            get => m_currentSkinView;
            set => m_currentSkinView = value;
        }

        #endregion

        #region Logic

        private void Start()
        {
            GetReferences();
        }

        public void Init(CameraMode cameraMode)
        {
            GetReferences();
            ChangeSkinViewByCameraMode(cameraMode);

            if (cameraMode == CameraMode.TPS)
            {
                // Unable the FPS skin view if the camera mode is TPS.
                TPSInit(cameraMode);
            }
            else if (cameraMode == CameraMode.FPS)
            {
                // Unable the TPS skin view if the camera mode is FPS.
                FPSInit();
            }
        }

        void GetReferences()
        {
            m_tpsPlayerController =
                GetSkinViewByCameraMode(CameraMode.TPS).SkinModel.GetComponent<TPSPlayerController>();
        }

        private void FPSInit()
        {
            m_tpsPlayerController.mesh.SetActive(false);
        }

        private void TPSInit(CameraMode cameraMode)
        {
            DisableAllOtherSkinViews();

            void DisableAllOtherSkinViews()
            {
                foreach (var skinView in _skinViews)
                {
                    if (skinView.Key != cameraMode)
                    {
                        skinView.Value.SkinModel.SetActive(false);
                    }
                }
            }
        }

        public SkinView ChangeSkinViewByCameraMode(CameraMode cameraMode)
        {
            SkinView skinView = GetSkinViewByCameraMode(cameraMode);
            if (skinView == null) return null;
            switch (cameraMode)
            {
                case CameraMode.TPS:
                    if (m_currentSkinView != null) m_currentSkinView.SkinModel.SetActive(false);
                    m_tpsPlayerController.mesh.SetActive(true);
                    break;
                case CameraMode.FPS:
                    m_tpsPlayerController.mesh.SetActive(false);
                    break;
                default:
                    Debug.LogError("Camera mode not implemented " + cameraMode);
                    break;
            }

            m_currentCameraMode = cameraMode;
            m_currentSkinView = skinView;
            m_currentSkinView.SkinModel.SetActive(true);
            return m_currentSkinView;
        }

        public SkinView GetSkinViewByCameraMode(CameraMode cameraMode)
        {
            foreach (var skinView in _skinViews)
            {
                if (skinView.Key == cameraMode)
                {
                    return skinView.Value;
                }
            }

            return null;
        }

        #endregion
    }
}