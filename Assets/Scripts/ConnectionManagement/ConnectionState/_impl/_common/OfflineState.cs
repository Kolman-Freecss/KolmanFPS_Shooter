#region

using ConnectionManagement.ConnectionState._impl.client;
using ConnectionManagement.ConnectionState._impl.host;
using Unity.Netcode;

#endregion

namespace ConnectionManagement.ConnectionState._impl._common
{
    /// <summary>
    /// Connection state corresponding to when the NetworkManager is shut down. From this state we can transition to the ClientConnecting sate, if starting as a client, or the StartingHost state, if starting as a host.
    /// </summary>
    public class OfflineState : ConnectionState
    {
        public OfflineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            NetworkManager.Singleton.Shutdown();
        }

        public override void Exit()
        {
        }

        public override void StartClientIP(string playerName, string ipaddress, int port)
        {
            ConnectionMethod connectionMethod =
                new ConnectionMethod(m_ConnectionManager, playerName, ipaddress, (ushort)port);
            m_ConnectionManager.ChangeState(new ClientConnectingState(m_ConnectionManager, connectionMethod));
        }

        public override void StartHostIP(string playerName, string ipaddress, int port)
        {
            ConnectionMethod connectionMethod =
                new ConnectionMethod(m_ConnectionManager, playerName, ipaddress, (ushort)port);
            m_ConnectionManager.ChangeState(new StartingHostState(m_ConnectionManager, connectionMethod));
        }
    }
}