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

        #endregion

        #region InitData

        private void Awake()
        {
            _playerInputs = new PlayerInputs();
        }

        private void Start()
        {
            SubscribeToDelegatesAndUpdateValues();
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

        #endregion

        #region Component Enable / Disable Events

        private void OnEnable()
        {
            _playerInputs.Enable();
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
        }

        #endregion
    }
}