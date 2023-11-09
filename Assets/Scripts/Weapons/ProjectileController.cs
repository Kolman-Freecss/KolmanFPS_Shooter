using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Weapons
{
    public class ProjectileController : NetworkBehaviour
    {

        #region Inspector Variables

        [SerializeField] private float shootForce;

        #endregion

        #region Auxiliar variables

        [HideInInspector]
        public PlayerBehaviour parent;
        private Rigidbody rb; 

        #endregion

        #region InitData

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        #endregion

        #region Loop

        // void Update()
        // {
        //     rb.velocity = rb.transform.forward * shootForce;
        // }

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