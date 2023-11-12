using Config;
using Gameplay.GameplayObjects;
using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector Fields


        #endregion

        #region Member Properties

        private Transform player;
        NetworkLifeState m_NetworkLifeState; 

        #endregion

        #region InitData

        void Start()
        {
            GetReferences();
        }

        void GetReferences()
        {
            player = transform;
            m_NetworkLifeState = GetComponent<NetworkLifeState>();
        }

        #endregion

        #region Loop

        void Update()
        {
            if (!GameManager.Instance.isGameStarted.Value
                || m_NetworkLifeState.LifeState.Value == LifeState.Dead
                ) return;
            Vector3 rot = player.GetComponent<PlayerController>().MainCamera.transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(0f, rot.y, 0f);
        }

        #endregion
    }
}