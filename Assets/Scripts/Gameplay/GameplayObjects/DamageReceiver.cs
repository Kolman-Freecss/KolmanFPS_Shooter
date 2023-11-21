#region

using System;
using Gameplay.Player;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Gameplay.GameplayObjects
{
    public class DamageReceiver : NetworkBehaviour, IDamageable
    {
        public event Action<PlayerBehaviour, int> DamageReceived;

        [SerializeField] NetworkLifeState m_NetworkLifeState;

        /// <summary>
        /// PlayerBehaviour is the class that deals damage to this object
        /// </summary>
        /// <param name="inflicter"></param>
        /// <param name="damage"></param>
        public void ReceiveDamage(PlayerBehaviour inflicter, int damage)
        {
            if (IsDamageable())
            {
                DamageReceived?.Invoke(inflicter, damage);
            }
        }

        /// <summary>
        /// If the gameObject is alive, it can be damaged
        /// </summary>
        /// <returns></returns>
        public bool IsDamageable()
        {
            return m_NetworkLifeState.LifeState.Value == LifeState.Alive;
        }
    }
}