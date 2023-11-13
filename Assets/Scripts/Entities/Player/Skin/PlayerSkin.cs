using System.Collections.Generic;
using System.ComponentModel;
using Entities.Camera;
using Entities.Utils;
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
        
        private SkinView m_currentSkinView;

        public SkinView CurrentSkinViewValue
        {
            get => m_currentSkinView;
            set => m_currentSkinView = value;
        }

        #endregion

        #region Logic

        public void Init(CameraMode cameraMode)
        {
            ChangeSkinViewByCameraMode(cameraMode);
        }

        public SkinView ChangeSkinViewByCameraMode(CameraMode cameraMode)
        {
            SkinView skinView = GetSkinViewByCameraMode(cameraMode);
            if (skinView == null) return null;
            
            if (m_currentSkinView != null) m_currentSkinView.SkinModel.SetActive(false);
            m_currentCameraMode = cameraMode;
            m_currentSkinView = skinView;
            m_currentSkinView.SkinModel.SetActive(true);
            return m_currentSkinView;
        }
        
        private SkinView GetSkinViewByCameraMode(CameraMode cameraMode)
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