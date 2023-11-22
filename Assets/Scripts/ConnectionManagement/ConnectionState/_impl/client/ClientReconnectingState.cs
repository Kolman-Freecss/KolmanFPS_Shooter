#region

using System.Collections;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace ConnectionManagement.ConnectionState._impl.client
{
    public class ClientReconnectingState : ClientConnectingState
    {
        Coroutine m_ReconnectCoroutine;
        int m_ReconnectAttemptCount;

        const float k_TimeBetweenAttempts = 5;

        public ClientReconnectingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void OnClientConnected(ulong _)
        {
            m_ConnectionManager.ChangeState(new ClientConnectedState(m_ConnectionManager));
        }

        public override void Enter()
        {
            m_ReconnectAttemptCount = 0;
            m_ReconnectCoroutine = m_ConnectionManager.StartCoroutine(ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (m_ReconnectCoroutine != null)
            {
                m_ConnectionManager.StopCoroutine(m_ReconnectCoroutine);
                m_ReconnectCoroutine = null;
            }
        }

        IEnumerator ReconnectCoroutine()
        {
            if (m_ReconnectAttemptCount > 0)
            {
                yield return new WaitForSeconds(k_TimeBetweenAttempts);
            }

            Debug.Log("Lost connection to host, trying to reconnect...");

            NetworkManager.Singleton.Shutdown();

            yield return
                new WaitWhile(() =>
                    NetworkManager.Singleton
                        .ShutdownInProgress); // wait until NetworkManager completes shutting down

            Debug.Log(
                $"Reconnecting attempt {m_ReconnectAttemptCount + 1}/{m_ConnectionManager.NbReconnectAttempts}...");
            m_ReconnectAttemptCount++;

            var connectingClient = ConnectClientAsync();
            yield return new WaitUntil(() => connectingClient.IsCompleted);
        }
    }
}