#region

using Unity.Netcode;

#endregion

namespace ConnectionManagement.ConnectionState
{
    /// <summary>
    /// Base class for connection states.
    /// </summary>
    public abstract class ConnectionState
    {
        protected ConnectionManager m_ConnectionManager;

        public ConnectionState(ConnectionManager connectionManager)
        {
            m_ConnectionManager = connectionManager;
        }

        #region Abstract Methods

        public abstract void Enter();

        public abstract void Exit();

        #endregion

        #region Virtual Methods

        public virtual void OnClientConnected(ulong clientId)
        {
        }

        public virtual void OnClientDisconnect(ulong clientId)
        {
        }

        public virtual void StartClientIP(string playerName, string ipaddress, int port)
        {
        }

        public virtual void StartHostIP(string playerName, string ipaddress, int port)
        {
        }

        public virtual void OnServerStarted()
        {
        }

        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
        }

        public virtual void OnTransportFailure()
        {
        }

        #endregion
    }
}