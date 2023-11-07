using Config;
using UnityEngine;

namespace Player
{
    public class CameraController : MonoBehaviour
    {
        #region Inspector Fields


        #endregion

        #region Private Properties

        private Transform player;

        #endregion

        #region InitData

        void Start()
        {
            GetReferences();
        }

        void GetReferences()
        {
            player = transform;
        }

        #endregion

        #region Loop

        void Update()
        {
            if (!GameManager.Instance.isGameStarted.Value) return;
            Vector3 rot = player.GetComponent<PlayerController>().MainCamera.transform.localRotation.eulerAngles;
            transform.localRotation = Quaternion.Euler(0f, rot.y, 0f);
        }

        #endregion
    }
}