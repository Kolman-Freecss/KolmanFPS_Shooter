using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerController : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Player")] [Tooltip("Movement speed of the player")] [SerializeField]
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
            GetReferences();
        }

        void GetReferences()
        {
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            _playerInputController = PlayerInputController.Instance;
            _controller = GetComponent<CharacterController>();
        }

        void Start()
        {
            SubscribeToDelegatesAndUpdateValues();
        }

        void SubscribeToDelegatesAndUpdateValues()
        {
        }

        #endregion


        #region Loop

        void Update()
        {
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

            Debug.Log(_playerInputController.move);
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
    }
}