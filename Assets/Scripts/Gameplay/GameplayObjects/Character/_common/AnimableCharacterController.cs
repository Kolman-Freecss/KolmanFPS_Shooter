#region

#endregion

#region

using Unity.Netcode;
using UnityEngine;

#endregion

namespace Gameplay.GameplayObjects.Character._common
{
    public abstract class AnimableCharacterController : NetworkBehaviour
    {
        #region Inspector Variables

        [SerializeField] Animator m_animator;

        #endregion

        #region Member Variables
        
        //protected float _animationBlend;

        protected bool m_isGrounded;

        private bool m_hasAnimator;

        private int _animIDForwardVelocity;

        private int _animIDBackwardVelocity;

        private int _animIDNormalizedVerticalVelocity;

        private int _animIDIsGrounded;

        #endregion

        #region Logic

        protected virtual void AssignAnimationIDs()
        {
            _animIDForwardVelocity = Animator.StringToHash("ForwardVelocity");
            _animIDBackwardVelocity = Animator.StringToHash("BackwardVelocity");
            _animIDNormalizedVerticalVelocity = Animator.StringToHash("NormalizedVerticalVelocity");
            _animIDIsGrounded = Animator.StringToHash("IsGrounded");
        }

        #endregion


        #region Getters & Setters

        public Animator Animator
        {
            get => m_animator;
            set => m_animator = value;
        }

        public bool HasAnimator
        {
            get => m_hasAnimator;
            set => m_hasAnimator = value;
        }

        public int AnimIDForwardVelocity => _animIDForwardVelocity;
        public int AnimIDBackwardVelocity => _animIDBackwardVelocity;
        public int AnimIDNormalizedVerticalVelocity => _animIDNormalizedVerticalVelocity;
        public int AnimIDIsGrounded => _animIDIsGrounded;

        #endregion
    }
}