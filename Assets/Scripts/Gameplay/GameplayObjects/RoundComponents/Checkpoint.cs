#region

using UnityEngine;

#endregion

namespace Gameplay.GameplayObjects.RoundComponents
{
    public class Checkpoint : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField] private GameObject checkpointCoordinates;
        [SerializeField] private Entities.Player.Player.TeamType teamType;

        #endregion

        #region Getter

        public Vector3 CheckpointCoordinatesPositionValue
        {
            get => checkpointCoordinates.transform.position;
        }

        public Entities.Player.Player.TeamType TeamTypeValue
        {
            get => teamType;
        }

        #endregion
    }
}