using System.Collections.Generic;
using Config;
using Entities.Camera;
using Entities.Player.Skin;
using Gameplay.GameplayObjects;
using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector Fields

        #endregion

        #region Member Properties

        PlayerController m_PlayerController;
        NetworkLifeState m_NetworkLifeState;
        CameraMode m_CurrentCameraMode = CameraMode.FPS;

        public CameraMode CurrentCameraModeValue
        {
            get => m_CurrentCameraMode;
            set => m_CurrentCameraMode = value;
        }

        #endregion

        #region InitData

        void Start()
        {
            GetReferences();
        }

        void GetReferences()
        {
            m_PlayerController = GetComponent<PlayerController>();
            m_NetworkLifeState = GetComponent<NetworkLifeState>();
            SetCameraModeByPlayer(m_CurrentCameraMode, m_PlayerController.Player);
        }

        #endregion

        #region Loop

        void Update()
        {
            if (!RoundManager.Instance.isRoundStarted.Value
                || m_NetworkLifeState.LifeState.Value == LifeState.Dead
               ) return;
            Vector3 rot = m_PlayerController.MainCamera.transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(0f, rot.y, 0f);
        }

        #endregion

        #region Logic

        public Entities.Player.Player SetCameraModeByPlayer(CameraMode mode, Entities.Player.Player currentPlayer)
        {
            currentPlayer.ChangeCurrentSkinView(mode, out SkinView skinView);
            if (skinView == null) throw new KeyNotFoundException("No typeSkin view available for this camera mode");
            m_CurrentCameraMode = mode;
            return currentPlayer;
        }

        #endregion
    }
}