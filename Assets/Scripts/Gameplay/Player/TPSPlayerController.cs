#region

using UnityEngine;

#endregion

namespace Gameplay.Player
{
    public class TPSPlayerController : MonoBehaviour
    {
        #region Inspector Variables

        public GameObject mesh;

        #endregion

        #region Member Variables

        private PlayerController m_playerController;

        public PlayerController PlayerControllerValue
        {
            get => m_playerController;
            set => m_playerController = value;
        }

        Animator m_animator;
        private bool _hasAnimator;

        #endregion

        #region InitData

        private void Awake()
        {
            enabled = false;
            GetReferences();
        }

        void GetReferences()
        {
            _hasAnimator = TryGetComponent(out m_animator);
        }

        #endregion

        #region Loop

        private void LateUpdate()
        {
            if (m_playerController == null) return;

            // Copy the player controller animator values to this animator.
            //TODO: Implement another different animator for the FPS player.
            if (_hasAnimator && m_playerController.HasAnimator)
            {
                m_animator.SetFloat(m_playerController.AnimIDForwardVelocity,
                    m_playerController.Animator.GetFloat(m_playerController.AnimIDForwardVelocity));
                m_animator.SetFloat(m_playerController.AnimIDBackwardVelocity,
                    m_playerController.Animator.GetFloat(m_playerController.AnimIDBackwardVelocity));
                m_animator.SetFloat(m_playerController.AnimIDNormalizedVerticalVelocity,
                    m_playerController.Animator.GetFloat(m_playerController.AnimIDNormalizedVerticalVelocity));
                m_animator.SetBool(m_playerController.AnimIDIsGrounded,
                    m_playerController.Animator.GetBool(m_playerController.AnimIDIsGrounded));
            }

            #endregion
        }
    }
}