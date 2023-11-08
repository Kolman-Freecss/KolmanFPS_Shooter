using Camera;
using Cinemachine;
using Config;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerController : NetworkBehaviour
    {
        #region Inspector Variables

        [Header("Player")] 
        
        [Tooltip("Player FPS Camera center")]
        [SerializeField] private Transform _playerFpsCameraCenter;
        
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

        #region Auxiliary Variables

        PlayerInputController _playerInputController;
        CharacterController _controller;
        GameObject _mainCamera;
        [HideInInspector]
        public GameObject MainCamera => _mainCamera;
        CinemachineVirtualCamera _playerFpsCamera;
        [HideInInspector]
        public CinemachineVirtualCamera PlayerFpsCamera => _playerFpsCamera;

        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;

        // Jump
        // timeout deltatime
        private bool _isGrounded;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private float _terminalVelocity = 53.0f;

        #endregion

        #region InitData

        private void Awake()
        {
            GetComponentReferences();
        }

        void GetComponentReferences()
        {
            _playerInputController = GetComponent<PlayerInputController>(); 
            _controller = GetComponent<CharacterController>();
        }
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                RoundManager.OnRoundManagerSpawned += () => transform.position = RoundManager.Instance.GetRandomCheckpoint().transform.position;
                RegisterServerCallbacks();
            }
            else
            {
                Debug.Log("Client");    
            }
            
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.InGame);
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
            if (!GameManager.Instance.isGameStarted.Value) return;
            Jump();
            GroundCheck();
            Move();
        }

        #endregion

        #region Logic

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
                    // update animator if using character
                    // if (_hasAnimator)
                    // {
                    //     _animator.SetBool(_animIDJump, true);
                    // }

                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
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
        }

        void GroundCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            _isGrounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
            // if (_hasAnimator)
            // {
            //     _animator.SetBool(_animIDOnGround, Grounded);
            // }
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

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            currentHorizontalSpeed = _mainCamera.transform.forward * currentHorizontalSpeed.z +
                                     _mainCamera.transform.right * currentHorizontalSpeed.x;
            currentHorizontalSpeed.y = 0.0f;
            float currentHorizontalSpeedMagnitude = currentHorizontalSpeed.magnitude;
            
            _controller.Move(targetDirection.normalized *
                             (currentHorizontalSpeedMagnitude * Time.deltaTime * targetSpeed) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        #endregion

        #region Network Calls/Events

        // This is called when a client connects to the server
        // Invoked when a client has loaded this scene
        private void ClientLoadedGameScene(ulong clientId)
        {
            if (IsServer)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] {clientId}
                    }
                };
                SendClientInitDataClientRpc(clientId, clientRpcParams);
            }
        }
        
        [ClientRpc]
        private void SendClientInitDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log("------------------SENT Client Init Awake Data------------------");
            Debug.Log("Client Id -> " + clientId);
            GameManager.Instance.AddPlayer(NetworkObjectId, this);
            if (!IsLocalPlayer || !IsOwner)
            {
                // We need to disable the player input controller for the other clients in every player
                GetComponent<PlayerInput>().enabled = false;
                GetComponent<CameraController>().enabled = false;
                GetComponent<PlayerInputController>().enabled = false;
                enabled = false;
                return;
            }
            InitClientData(clientId);
        }

        public void InitClientData(ulong clientId)
        {
            GetSceneReferences();
        }
        
        void GetSceneReferences()
        {
            if (_mainCamera == null)
            {
                _mainCamera = RoundManager.Instance.GetMainCamera().gameObject;
            }
            this._playerFpsCamera = RoundManager.Instance.GetPlayerFPSCamera();
            if (this._playerFpsCamera != null)
            {
                this._playerFpsCamera.Follow = _playerFpsCameraCenter;
                this._playerFpsCamera.LookAt = _playerFpsCameraCenter;
                this._playerFpsCamera.GetComponent<CinemachinePOVExtension>().SetPlayer(_playerInputController); 
            }
            else
            {
                Debug.LogWarning("Player FPS Camera not found");
            }
            GetComponent<PlayerInput>().enabled = true;
            GetComponent<CameraController>().enabled = true;
            GetComponent<PlayerInputController>().enabled = true;
            enabled = true;
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
                GameManager.Instance.RemovePlayerAllClients(NetworkObjectId);
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