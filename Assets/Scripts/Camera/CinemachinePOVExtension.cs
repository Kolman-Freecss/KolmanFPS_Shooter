using Cinemachine;
using Player;
using UnityEngine;

namespace Camera
{
    public class CinemachinePOVExtension : CinemachineExtension
    {
        #region Auxiliary Variables

        // Sensitivity X
        [SerializeField] private float _horizontalSpeed = 1.0f;

        // Sensitivity Y
        [SerializeField] private float _verticalSpeed = 1.0f;

        [SerializeField] private float _clampAngle = 80.0f;

        private PlayerInputController _playerInputs;
        private Vector3 _startingRotation;

        #endregion

        #region InitData

        protected override void Awake()
        {
            base.Awake();
            // GetReferences();
        }

        #endregion

        #region Logic

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state, float deltaTime)
        {
            if (vcam.Follow)
            {
                if (stage == CinemachineCore.Stage.Aim)
                {
                    if (_startingRotation == null) _startingRotation = transform.localRotation.eulerAngles;
                    if (_playerInputs == null) return;
                    Vector2 delta = _playerInputs.GetMouseDelta();
                    _startingRotation.x += delta.x * Time.deltaTime * _verticalSpeed;
                    _startingRotation.y += delta.y * Time.deltaTime * _horizontalSpeed;
                    _startingRotation.y = Mathf.Clamp(_startingRotation.y, -_clampAngle, _clampAngle);
                    state.RawOrientation = Quaternion.Euler(-_startingRotation.y, _startingRotation.x, 0f);
                }
            }
        }

        public void SetPlayer(PlayerInputController player)
        {
            this._playerInputs = player;
        }

        #endregion
    }
}