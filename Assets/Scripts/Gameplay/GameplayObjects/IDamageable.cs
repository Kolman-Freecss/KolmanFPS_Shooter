#region

using Gameplay.Player;
using UnityEngine;

#endregion

namespace Gameplay.GameplayObjects
{
    public interface IDamageable
    {
        void ReceiveDamage(PlayerBehaviour inflicter, int damage);

        ulong NetworkObjectId { get; }

        Transform transform { get; }

        bool IsDamageable();
    }
}