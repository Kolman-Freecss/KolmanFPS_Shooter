using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputController : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Movement Settings")] public bool analogMovement;

        #endregion

        #region Auxiliar Variables

        private PlayerInputs _playerInputs;
        private static PlayerInputController _instance;
        [HideInInspector]
        public static PlayerInputController Instance => _instance;

        [HideInInspector] public Vector2 move;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool sprint;
        [HideInInspector] public bool leftClick;
        [HideInInspector] public bool rightClick;

        #endregion

        #region InitData

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
            Cursor.visible = false;
            GetReferences();
        }
        
        private void GetReferences()
        {
            _playerInputs = new PlayerInputs();
        }

        private void SubscribeToDelegatesAndUpdateValues()
        {
            _playerInputs.Desktop.Move.started += OnMove;
            _playerInputs.Desktop.Move.performed += OnMove;
            _playerInputs.Desktop.Move.canceled += OnMove;

            _playerInputs.Desktop.Jump.started += OnJump;
            _playerInputs.Desktop.Jump.performed += OnJump;
            _playerInputs.Desktop.Jump.canceled += OnJump;

            _playerInputs.Desktop.Sprint.started += OnSprint;
            _playerInputs.Desktop.Sprint.performed += OnSprint;
            _playerInputs.Desktop.Sprint.canceled += OnSprint;
            
            _playerInputs.Desktop.Shoot.started += OnLeftClick;
            _playerInputs.Desktop.Shoot.performed += OnLeftClick;
            _playerInputs.Desktop.Shoot.canceled += OnLeftClick;
            
            _playerInputs.Desktop.Aim.started += OnRightClick;
            _playerInputs.Desktop.Aim.performed += OnRightClick;
            _playerInputs.Desktop.Aim.canceled += OnRightClick;
            
        }

        #endregion

        #region InputSystem Events

        public void OnMove(InputAction.CallbackContext value)
        {
            Debug.Log("Move");
            MoveInput(value.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext value)
        {
            Debug.Log("Jump");
            JumpInput(value.ReadValue<float>());
        }

        public void OnSprint(InputAction.CallbackContext value)
        {
            sprint = value.ReadValueAsButton();
        }
        
        public void OnLeftClick(InputAction.CallbackContext value)
        {
            leftClick = value.ReadValueAsButton();
        }
        
        public void OnRightClick(InputAction.CallbackContext value)
        {
            rightClick = value.ReadValueAsButton();
        }

        private void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        private void JumpInput(float newJumpState)
        {
            if (newJumpState > 0)
            {
                jump = true;
            }
            else
            {
                jump = false;
            }
        }

        private void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }
        
        private void LeftClickInput(bool newLeftClickState)
        {
            leftClick = newLeftClickState;
        }
        
        private void RightClickInput(bool newRightClickState)
        {
            rightClick = newRightClickState;
        }

        #endregion

        #region Component Enable / Disable Events

        private void OnEnable()
        {
            _playerInputs.Enable();
            SubscribeToDelegatesAndUpdateValues();
        }

        private void OnDisable()
        {
            _playerInputs.Disable();

            UnsubscribeToDelegates();
        }

        private void UnsubscribeToDelegates()
        {
            _playerInputs.Desktop.Move.started -= OnMove;
            _playerInputs.Desktop.Move.performed -= OnMove;

            _playerInputs.Desktop.Jump.started -= OnJump;
            _playerInputs.Desktop.Jump.performed -= OnJump;

            _playerInputs.Desktop.Sprint.started -= OnSprint;
            _playerInputs.Desktop.Sprint.performed -= OnSprint;
            
            _playerInputs.Desktop.Shoot.started -= OnLeftClick;
            _playerInputs.Desktop.Shoot.performed -= OnLeftClick;
            
            _playerInputs.Desktop.Aim.started -= OnRightClick;
            _playerInputs.Desktop.Aim.performed -= OnRightClick;
        }

        #endregion

        #region Getters

        public Vector2 GetMouseDelta()
        {
            return _playerInputs.Desktop.Look.ReadValue<Vector2>();
        }

        #endregion
    }
}