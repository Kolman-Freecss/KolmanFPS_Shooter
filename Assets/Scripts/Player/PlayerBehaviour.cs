using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerBehaviour))]
    public class PlayerBehaviour : MonoBehaviour
    {

        #region Inspector variables

        [Header("Player")]
        [Tooltip("Max health of the player")]
        [SerializeField] private float _maxHealth = 100f;

        #endregion

        #region Auxiliary Variables

        private float _health = 100f;

        #endregion

        #region InitData

        

        #endregion

        #region Logic

        

        #endregion
        
    }
}