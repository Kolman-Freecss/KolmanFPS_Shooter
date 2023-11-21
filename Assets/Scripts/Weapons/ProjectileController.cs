#region

using Player;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Weapons
{
    public class ProjectileController : NetworkBehaviour
    {
        #region Inspector Variables

        [SerializeField] private float shootForce;

        #endregion

        #region Auxiliar variables

        [HideInInspector] public PlayerBehaviour parent;
        private Rigidbody rb;

        #endregion

        #region InitData

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        #endregion

        #region Loop

        #endregion

        #region Events

        private void OnTriggerEnter(Collider other)
        {
            if (!IsOwner) return;

            parent.DestroyProjectileServerRpc(NetworkObjectId);
        }

        #endregion
    }
}