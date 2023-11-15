using System;
using Camera;
using Cinemachine;
using Config;
using Entities.Camera;
using Gameplay.GameplayObjects;
using Gameplay.Player;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        #region Inspector Variables

        [Header("Player")] 
        
        [FormerlySerializedAs("Skin")]
        [Tooltip("typeSkin")]
        [SerializeField] public Entities.Player.Player.PlayerTypeSkin typeSkin = Entities.Player.Player.PlayerTypeSkin.DefaultSkin;
        
        [Tooltip("Movement speed of the player")] [SerializeField]
        private float _speed = 6f;

        [Tooltip("Sprint speed of the player")] [SerializeField]
        private float _sprintSpeed = 12f;

        [Tooltip("How fast the character turns to face movement direction")] [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Space(10)] [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        
        [Header("Player Grounded")] [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Tooltip("Useful for rough ground")] public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        #endregion

        #region Member Variables

        Entities.Player.Player m_player;
        public Entities.Player.Player Player => m_player;
        
        PlayerInputController _playerInputController;
        TPSPlayerController m_tpsPlayerController;
        CharacterController _controller;
        PlayerBehaviour m_playerBehaviour;
        Animator _animator;
        public Animator Animator => _animator;
        
        //Camera
        GameObject _mainCamera;
        public GameObject MainCamera => _mainCamera;
        CinemachineVirtualCamera _playerFpsCamera;
        public CinemachineVirtualCamera PlayerFpsCamera => _playerFpsCamera;

        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        
        private float _animationBlend;

        // Jump
        // timeout deltatime
        private bool _isGrounded;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _terminalVelocity = 53.0f;
        
        //Animator
        private bool _hasAnimator;
        public bool HasAnimator => _hasAnimator;
        
        private int _animIDForwardVelocity;
        public int AnimIDForwardVelocity => _animIDForwardVelocity;
        private int _animIDBackwardVelocity;
        public int AnimIDBackwardVelocity => _animIDBackwardVelocity;
        private int _animIDNormalizedVerticalVelocity;
        public int AnimIDNormalizedVerticalVelocity => _animIDNormalizedVerticalVelocity;
        private int _animIDIsGrounded;
        public int AnimIDIsGrounded => _animIDIsGrounded;

        #endregion

        #region InitData

        private void Awake()
        {
            GetComponentReferences();
            AssignAnimationIDs();
        }

        void GetComponentReferences()
        {
            _playerInputController = GetComponent<PlayerInputController>(); 
            _controller = GetComponent<CharacterController>();
            m_playerBehaviour = GetComponent<PlayerBehaviour>();
        }

        void AssignAnimationIDs()
        {
            _animIDForwardVelocity = Animator.StringToHash("ForwardVelocity");
            _animIDBackwardVelocity = Animator.StringToHash("BackwardVelocity");
            _animIDNormalizedVerticalVelocity = Animator.StringToHash("NormalizedVerticalVelocity");
            _animIDIsGrounded = Animator.StringToHash("IsGrounded");
        }
        
        public override void OnNetworkSpawn()
        {
            // This is called when the local player is spawned and will be enabled after the scene is loaded
            enabled = false;
            if (IsServer)
            {
                RoundManager.OnRoundManagerSpawned += () =>
                {
                    Transform checkpoint = RoundManager.Instance.GetRandomCheckpoint().transform;
                    transform.position = new Vector3(checkpoint.position.x, checkpoint.position.y + 1f, checkpoint.position.z);
                };
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
            //Server will be notified when a client connects
            SceneTransitionHandler.Instance.OnClientLoadedGameScene += ClientLoadedGameScene;
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
            // Debug.Log("Update GameManager.Instance.isGameStarted.Value= " + GameManager.Instance.isGameStarted.Value + " GameManager.isGameStarted.Value= " + GameManager.isGameStarted.Value);
            if (!GameManager.Instance.isGameStarted.Value || m_playerBehaviour.LifeState == LifeState.Dead) return;
            Jump();
            GroundCheck();
            Move();
        }

        // private void LateUpdate()
        // {
        //     if (!GameManager.Instance.isGameStarted.Value || m_playerBehaviour.LifeState == LifeState.Dead) return;
        //     
        // }

        #endregion

        #region Logic

        public void OnDead()
        {
            // if (_hasAnimator)
            // {
            //     _animator.SetTrigger("Dead");
            // }
            RoundManager.Instance.OnPlayerDeathServerRpc(NetworkObjectId);
        }

        void Jump()
        {
            if (_isGrounded)
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

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDNormalizedVerticalVelocity, _verticalVelocity / JumpHeight);
            }
            
        }

        void GroundCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDIsGrounded, _isGrounded);
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
            
            _controller.Move(targetDirection.normalized *
                             (currentHorizontalSpeedMagnitude * Time.deltaTime * targetSpeed) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            
            if (_hasAnimator)
            {
                Vector3 localSmoothedAnimationVelocity = transform.InverseTransformDirection(currentHorizontalSpeed);
                _animator.SetFloat(_animIDForwardVelocity, localSmoothedAnimationVelocity.z);
                _animator.SetFloat(_animIDBackwardVelocity, localSmoothedAnimationVelocity.x);
            }
            
        }

        #endregion

        #region Network Calls/Events

        // This is called when a client connects to the server
        // Invoked when a client has loaded this scene
        private void ClientLoadedGameScene(ulong clientId)
        {
            if (IsServer)
            {
                SendClientInitDataClientRpc(clientId);
            }
        }
        
        [ClientRpc]
        private void SendClientInitDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Init Awake Data------------------");
            Debug.Log("Client Id -> " + clientId);
            if (!IsLocalPlayer || !IsOwner)
            {
                InitOtherClientsData();
                return;
            }
            InitClientData(clientId);
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
            CreatePlayerReference(CameraMode.FPS, typeSkin, "DefaultNamePlayer",
                Entities.Player.Player.TeamType.Wizards,
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
            _animator = m_player.CurrentSkinModel.GetComponent<Animator>(); 
            _hasAnimator = _animator != null;
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

        #region Event Functions

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (_isGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
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
            RoundManager.OnRoundManagerSpawned -= () => transform.position = RoundManager.Instance.GetRandomCheckpoint().transform.position;
            SceneTransitionHandler.Instance.OnClientLoadedGameScene -= ClientLoadedGameScene;
        }
        
        void UnSubscribeToDelegatesAndUpdateValues()
        {
        }

        #endregion
        
    }
}