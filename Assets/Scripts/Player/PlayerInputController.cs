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

        [HideInInspector] public Vector2 move;
        [HideInInspector] public bool jump;
        [HideInInspector] public bool sprint;
        [HideInInspector] public bool leftClick;
        [HideInInspector] public bool rightClick;

        #endregion

        #region InitData

        private void Awake()
        {
            GetReferences();
        }
        
        private void OnEnable()
        {
            _playerInputs = new PlayerInputs();
            Cursor.visible = false;
            _playerInputs.Desktop.Enable();
            SubscribeToDelegatesAndUpdateValues();
        }
        
        private void GetReferences()
        {
            
        }

        private void SubscribeToDelegatesAndUpdateValues()
        {
            _playerInputs.Desktop.Move.performed += OnMove;
            _playerInputs.Desktop.Move.canceled += OnMove;

            _playerInputs.Desktop.Jump.started += OnJump;
            _playerInputs.Desktop.Jump.canceled += OnJump;

            _playerInputs.Desktop.Sprint.performed += OnSprint;
            _playerInputs.Desktop.Sprint.canceled += OnSprint;
            
            _playerInputs.Desktop.Shoot.performed += OnLeftClick;
            _playerInputs.Desktop.Shoot.canceled += OnLeftClick;
            
            _playerInputs.Desktop.Aim.started += OnRightClick;
            _playerInputs.Desktop.Aim.canceled += OnRightClick;
            
        }

        #endregion

        #region InputSystem Events

        public void OnMove(InputAction.CallbackContext value)
        {
            MoveInput(value.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext value)
        {
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

        #region Destructor

        private void OnDisable()
        {
            _playerInputs.Desktop.Disable();

            UnsubscribeToDelegates();
        }

        private void UnsubscribeToDelegates()
        {
            _playerInputs.Desktop.Move.performed -= OnMove;
            _playerInputs.Desktop.Move.canceled -= OnMove;

            _playerInputs.Desktop.Jump.started -= OnJump;
            _playerInputs.Desktop.Jump.canceled -= OnJump;

            _playerInputs.Desktop.Sprint.performed -= OnSprint;
            _playerInputs.Desktop.Sprint.canceled -= OnSprint;
            
            _playerInputs.Desktop.Shoot.performed -= OnLeftClick;
            _playerInputs.Desktop.Shoot.canceled -= OnLeftClick;
            
            _playerInputs.Desktop.Aim.started -= OnRightClick;
            _playerInputs.Desktop.Aim.canceled -= OnRightClick;
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