#region

using System.Text;
using System.Threading.Tasks;
using ConnectionManagement.model;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

#endregion

namespace ConnectionManagement
{
    public class ConnectionMethod
    {
        #region Member properties

        private ConnectionManager m_ConnectionManager;
        private string m_playerName;
        private string m_ipaddress;
        private ushort m_port;

        #endregion

        public ConnectionMethod(ConnectionManager connectionManager, string playerName, string ipaddress, ushort port)
        {
            m_ConnectionManager = connectionManager;
            m_playerName = playerName;
            m_ipaddress = ipaddress;
            m_port = port;
        }

        public async Task SetupClientConnectionAsync()
        {
            SetConnectionPayload(m_playerName);
            UnityTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            if (transport == null)
            {
                Debug.LogError("Transport is not set to UnityTransport!");
                return;
            }

            transport.SetConnectionData(m_ipaddress, m_port);
        }

        public async Task SetupHostConnectionAsync()
        {
            SetConnectionPayload(m_playerName);
            UnityTransport transport = NetworkManager.Singleton.NetworkConfig.NetworkTransport as UnityTransport;
            if (transport == null)
            {
                Debug.LogError("Transport is not set to UnityTransport!");
                return;
            }

            transport.SetConnectionData(m_ipaddress, m_port);
        }

        private void SetConnectionPayload(string playerName)
        {
            string playerId = NetworkManager.Singleton.LocalClientId.ToString() + m_playerName;
            string payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                playerId = playerId,
                playerName = playerName,
            });

            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(payload);
        }
    }
}