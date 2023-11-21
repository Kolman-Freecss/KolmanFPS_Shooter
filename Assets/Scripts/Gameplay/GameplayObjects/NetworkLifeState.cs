#region

using Unity.Netcode;
using UnityEngine;

#endregion

namespace Gameplay.GameplayObjects
{
    public enum LifeState
    {
        Alive,
        Dead,
    }

    public class NetworkLifeState : NetworkBehaviour
    {
        [SerializeField] NetworkVariable<LifeState> m_LifeState = new NetworkVariable<LifeState>(
            GameplayObjects.LifeState.Alive,
            NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

        public NetworkVariable<LifeState> LifeState => m_LifeState;
    }
}