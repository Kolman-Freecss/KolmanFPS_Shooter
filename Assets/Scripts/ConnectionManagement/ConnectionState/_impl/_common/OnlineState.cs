namespace ConnectionManagement.ConnectionState._impl._common
{
    /// <summary>
    /// Base class when a state is online connection
    /// </summary>
    public abstract class OnlineState : ConnectionState
    {
        public OnlineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void OnTransportFailure()
        {
            m_ConnectionManager.ChangeState(new OfflineState(m_ConnectionManager));
        }
    }
}