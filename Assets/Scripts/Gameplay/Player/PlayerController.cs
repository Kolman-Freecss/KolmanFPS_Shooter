#region

using System;
using Camera;
using Cinemachine;
using Entities.Camera;
using Gameplay.Config;
using Gameplay.GameplayObjects;
using Modules.CacheModule;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using CharacterController = Gameplay.GameplayObjects.Character._common.CharacterController;

#endregion

namespace Gameplay.Player
{
    public class PlayerController : CharacterController
    {
        #region Inspector Variables

        [Header("Player")] [FormerlySerializedAs("Skin")] [Tooltip("typeSkin")] [SerializeField]
        public Entities.Player.Player.PlayerTypeSkin typeSkin = Entities.Player.Player.PlayerTypeSkin.DefaultSkin;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        #endregion

        #region Member Variables

        Entities.Player.Player m_player;
        public Entities.Player.Player Player => m_player;

        PlayerInputController _playerInputController;
        TPSPlayerController m_tpsPlayerController;
        PlayerBehaviour m_playerBehaviour;

        //Camera
        GameObject _mainCamera;
        public GameObject MainCamera => _mainCamera;
        CinemachineVirtualCamera _playerFpsCamera;
        public CinemachineVirtualCamera PlayerFpsCamera => _playerFpsCamera;

        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;

        // Jump
        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _terminalVelocity = 53.0f;

        #endregion

        #region InitData

        private void Awake()
        {
            GetComponentReferences();
            base.AssignAnimationIDs();
        }

        protected override void GetComponentReferences()
        {
            base.GetComponentReferences();
            _playerInputController = GetComponent<PlayerInputController>();
            m_playerBehaviour = GetComponent<PlayerBehaviour>();
        }


        public override void OnNetworkSpawn()
        {
            // This is called when the local player is spawned and will be enabled after the scene is loaded
            enabled = false;
            if (IsServer)
            {
                RoundManager.OnRoundStarted += AssignPlayerCheckPoint;
                RegisterServerCallbacks();
            }
            else
            {
                Debug.Log("Client");
            }

            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.Multiplayer_InGame);
        }

        private void RegisterServerCallbacks()
        {
            GameManager.Instance.allPlayersSpawned += InitClientData;
        }

        void Start()
        {
            if (!IsLocalPlayer || !IsOwner) return;
            SubscribeToDelegatesAndUpdateValues();
        }

        void SubscribeToDelegatesAndUpdateValues()
        {
        }

        #endregion

        #region Loop

        void Update()
        {
            if (!RoundManager.Instance.isRoundStarted.Value || m_playerBehaviour.LifeState == LifeState.Dead) return;
            Jump();
            GroundCheck();
            Move();
        }

        #endregion

        #region Logic

        public void AssignPlayerCheckPoint()
        {
            Vector3 checkpoint = RoundManager.Instance.GetCheckpointCoordinates(Player.TeamTypeValue);
            transform.position = new Vector3(checkpoint.x, checkpoint.y + 1f,
                checkpoint.z);
        }

        public override void OnDead()
        {
            base.OnDead();
            RoundManager.Instance.OnPlayerDeathServerRpc(NetworkObjectId);
        }

        void Jump()
        {
            if (m_isGrounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // if (_hasAnimator)
                // {
                //     _animator.SetBool(_animIDJump, false);
                // }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_playerInputController.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    // if (_hasAnimator)
                    // {
                    //     _animator.SetBool(_animIDJump, true);
                    // }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                // else
                // {
                //     // update animator if using character
                //     if (_hasAnimator)
                //     {
                //         _animator.SetBool(_animIDFreeFall, true);
                //     }
                // }

                // if we are not grounded, do not jump
                _playerInputController.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                // if (_hasAnimator)
                // {
                //     _animator.SetFloat(_animIDJumpVelocity, _verticalVelocity);
                // }
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            if (HasAnimator)
            {
                Animator.SetFloat(AnimIDNormalizedVerticalVelocity, _verticalVelocity / JumpHeight);
            }
        }

        void Move()
        {
            float targetSpeed = _playerInputController.sprint ? _sprintSpeed : _speed;

            if (_playerInputController.move == Vector2.zero) targetSpeed = 0.0f;

            Vector3 currentHorizontalSpeed =
                new Vector3(_playerInputController.move.x, 0.0f, _playerInputController.move.y);

            // normalise input direction
            Vector3 inputDirection = new Vector3(_playerInputController.move.x, 0.0f, _playerInputController.move.y)
                .normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_playerInputController.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            // _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime); //TODO: Acceleration * SpeedChangeRate
            // if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            currentHorizontalSpeed = _mainCamera.transform.forward * currentHorizontalSpeed.z +
                                     _mainCamera.transform.right * currentHorizontalSpeed.x;
            currentHorizontalSpeed.y = 0.0f;
            float currentHorizontalSpeedMagnitude = currentHorizontalSpeed.magnitude;

            m_controller.Move(targetDirection.normalized *
                              (currentHorizontalSpeedMagnitude * Time.deltaTime * targetSpeed) +
                              new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (HasAnimator)
            {
                Vector3 localSmoothedAnimationVelocity = transform.InverseTransformDirection(currentHorizontalSpeed);
                Animator.SetFloat(AnimIDForwardVelocity, localSmoothedAnimationVelocity.z);
                Animator.SetFloat(AnimIDBackwardVelocity, localSmoothedAnimationVelocity.x);
            }
        }

        #endregion

        #region Network Calls/Events

        private void InitClientData()
        {
            if (IsServer)
            {
                SendClientInitDataClientRpc();
            }
        }

        [ClientRpc]
        private void SendClientInitDataClientRpc(ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Init Awake Data------------------");
            Debug.Log("Client Id -> " + NetworkManager.Singleton.LocalClientId);
            if (!IsOwner) // || !IsLocalPlayer
            {
                InitOtherClientsData();
                return;
            }

            InitClientData(NetworkManager.Singleton.LocalClientId);
        }

        /// <summary>
        /// Init default values for the network player objects
        /// </summary>
        public void InitOtherClientsData()
        {
            // We need to disable the player input controller for the other clients in every player
            GetComponent<PlayerBehaviour>().enabled = false;
            GetComponent<PlayerInput>().enabled = false;
            GetComponent<CameraController>().enabled = false;
            GetComponent<PlayerInputController>().enabled = false;
            CreatePlayerReference(CameraMode.TPS, typeSkin, "DefaultNamePlayer",
                Entities.Player.Player.TeamType.Warriors,
                gameObject);
            enabled = false;
        }

        public void InitClientData(ulong clientId)
        {
            GetSceneReferences();
        }

        void GetSceneReferences()
        {
            string username =
                GameManager.Instance.CacheManagement.GetPlayerCache<string>(PlayerCache.PlayerCacheKeys.Username);
            Entities.Player.Player.TeamType teamType = (Entities.Player.Player.TeamType)Enum.Parse(
                typeof(Entities.Player.Player.TeamType),
                GameManager.Instance.CacheManagement.GetPlayerCache<string>(PlayerCache.PlayerCacheKeys.TeamType));
            Debug.Log("Username -> " + username + " TeamType -> " + teamType);
            CreatePlayerReference(CameraMode.FPS, typeSkin, username,
                teamType,
                gameObject);
            m_tpsPlayerController = m_player.GetTPSPlayerController();
            if (m_tpsPlayerController != null)
            {
                m_tpsPlayerController.PlayerControllerValue = this;
                m_tpsPlayerController.enabled = true;
            }

            if (_mainCamera == null)
            {
                _mainCamera = RoundManager.Instance.GetMainCamera().gameObject;
            }

            this._playerFpsCamera = RoundManager.Instance.GetPlayerFPSCamera();
            if (this._playerFpsCamera != null)
            {
                this._playerFpsCamera.Follow = m_player.Head;
                this._playerFpsCamera.LookAt = m_player.Head;
                this._playerFpsCamera.GetComponent<CinemachinePOVExtension>().SetPlayer(_playerInputController);
            }
            else
            {
                Debug.LogWarning("Player FPS Camera not found");
            }

            GetComponent<PlayerBehaviour>().enabled = true;
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<CameraController>().enabled = true;
            GetComponent<PlayerInputController>().enabled = true;
            Animator = m_player.CurrentSkinModel.GetComponent<Animator>();
            HasAnimator = Animator != null;
            m_playerBehaviour.InitRoundData();
            enabled = true;
        }

        private void CreatePlayerReference(CameraMode cameraMode,
            Entities.Player.Player.PlayerTypeSkin typeSkin,
            string name,
            Entities.Player.Player.TeamType teamType,
            GameObject gameObject)
        {
            m_player = PlayerFactory.CreatePlayer(
                cameraMode,
                typeSkin,
                name,
                teamType,
                gameObject);
            GetComponent<CameraController>().CurrentCameraModeValue = m_player.CurrentCameraMode;
        }

        #endregion

        #region Destructor

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsLocalPlayer)
            {
                GameManager.Instance.ClearInitData();
            }

            if (IsServer)
            {
                UnregisterServerCallbacks();
            }

            UnSubscribeToDelegatesAndUpdateValues();
        }

        private void UnregisterServerCallbacks()
        {
            //Server will be notified when a client connects
            RoundManager.OnRoundStarted -= AssignPlayerCheckPoint;
            GameManager.Instance.allPlayersSpawned -= InitClientData;
        }

        void UnSubscribeToDelegatesAndUpdateValues()
        {
        }

        #endregion
    }
}