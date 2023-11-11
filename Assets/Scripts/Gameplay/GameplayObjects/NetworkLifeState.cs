using Unity.Netcode;
using UnityEngine;

namespace Gameplay.GameplayObjects
{
    
    public enum LifeState
    {
        Alive,
        Dead,
    }
    
    public class NetworkLifeState : NetworkBehaviour
    {
        [SerializeField]
        NetworkVariable<LifeState> m_LifeState = new NetworkVariable<LifeState>(GameplayObjects.LifeState.Alive);

        public NetworkVariable<LifeState> LifeState => m_LifeState;
    }
}