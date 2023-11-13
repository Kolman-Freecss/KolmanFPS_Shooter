using System;
using System.Collections.Generic;
using Config;
using Entities.Camera;
using Entities.Player;
using Gameplay.GameplayObjects;
using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector Fields

        // [SerializeField] private Dictionary<CameraMode, GameObject> modelSourceForCameras;

        #endregion

        #region Member Properties

        PlayerController m_PlayerController;
        NetworkLifeState m_NetworkLifeState;
        CameraMode m_CurrentCameraMode = CameraMode.FPS;

        #endregion

        #region InitData

        // private void Awake()
        // {
        //     if (modelSourceForCameras == null || modelSourceForCameras.Count == 0)
        //     {
        //         modelSourceForCameras = new Dictionary<CameraMode, GameObject>()
        //         {
        //             {CameraMode.FPS, transform.Find("ModelFpsSourceForCamera").gameObject},
        //             {CameraMode.TPS, transform.Find("ModelTpsSourceForCamera").gameObject}
        //         };
        //     }
        // }

        void Start()
        {
            GetReferences();
        }

        void GetReferences()
        {
            m_PlayerController = GetComponent<PlayerController>();
            m_NetworkLifeState = GetComponent<NetworkLifeState>();
            SetCameraMode(m_CurrentCameraMode, m_PlayerController.Player);
        }

        #endregion

        #region Loop

        void Update()
        {
            if (!GameManager.Instance.isGameStarted.Value
                || m_NetworkLifeState.LifeState.Value == LifeState.Dead
               ) return;
            Vector3 rot = m_PlayerController.MainCamera.transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(0f, rot.y, 0f);
        }

        #endregion

        #region Logic

        public Entities.Player.Player SetCameraMode(CameraMode mode, Entities.Player.Player currentPlayer)
        {
            // SkinView currentModelToUse;
            currentPlayer.ChangeCurrentSkinView(mode, out SkinView skinView);
            // SwitchCamera(mode, currentPlayer, out currentModelToUse);
            if (skinView == null) throw new Exception("No skin view available for this camera mode");
            m_CurrentCameraMode = mode;
            // return currentModelToUse;
            return currentPlayer;

            Entities.Player.Player SwitchCamera(CameraMode cameraMode, Entities.Player.Player player, out SkinView modelToUse)
            {
                // CameraMode previousCameraMode = m_CurrentCameraMode;
                player.ChangeCurrentSkinView(cameraMode, out SkinView skinView);
                modelToUse = skinView;
                return player;


                // modelSourceForCameras.TryGetValue(previousCameraMode, out GameObject previousModelSourceForCamera);
                // modelToUse = previousModelSourceForCamera;
                // modelSourceForCameras.TryGetValue(cameraMode, out GameObject modelSourceForCamera);
                // if (modelSourceForCamera != null)
                // {
                //     modelSourceForCamera.SetActive(true);
                //     modelToUse = modelSourceForCamera;
                //     if (previousModelSourceForCamera != null) previousModelSourceForCamera.SetActive(false);
                // }
            }

            #endregion
        }
    }
}