#region

using System;
using System.Threading.Tasks;
using ConnectionManagement.ConnectionState._impl._common;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace ConnectionManagement.ConnectionState._impl.client
{
    public class ClientConnectingState : OnlineState
    {
        ConnectionMethod m_ConnectionMethod;

        public ClientConnectingState(ConnectionManager connectionManager) : base(
            connectionManager)
        {
        }

        public ClientConnectingState(ConnectionManager connectionManager, ConnectionMethod connectionMethod) : this(
            connectionManager)
        {
            m_ConnectionMethod = connectionMethod;
        }

        public override void Enter()
        {
            ConnectClientAsync();
        }

        public override void Exit()
        {
        }

        public override void OnClientConnected(ulong _)
        {
            m_ConnectionManager.ChangeState(new ClientConnectedState(m_ConnectionManager));
        }

        protected void StartingClientFailedAsync()
        {
            m_ConnectionManager.ChangeState(new OfflineState(m_ConnectionManager));
        }

        internal async Task ConnectClientAsync()
        {
            try
            {
                // Setup NGO with current connection method
                await m_ConnectionMethod.SetupClientConnectionAsync();

                // NGO's StartClient launches everything
                if (!NetworkManager.Singleton.StartClient())
                {
                    throw new Exception("NetworkManager StartClient failed");
                }

                Debug.Log("Client started");
            }
            catch (Exception e)
            {
                Debug.LogError("Error connecting client, see following exception");
                Debug.LogException(e);
                StartingClientFailedAsync();
                throw;
            }
        }
    }
}