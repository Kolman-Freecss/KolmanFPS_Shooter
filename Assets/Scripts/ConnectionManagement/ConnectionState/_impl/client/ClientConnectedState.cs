#region

using UnityEngine;

#endregion

namespace ConnectionManagement.ConnectionState._impl.client
{
    public class ClientConnectedState : ConnectionState
    {
        public ClientConnectedState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
        }

        public override void OnClientDisconnect(ulong _)
        {
            Debug.Log("Lost connection to host");
            m_ConnectionManager.ChangeState(new ClientReconnectingState(m_ConnectionManager));
        }
    }
}