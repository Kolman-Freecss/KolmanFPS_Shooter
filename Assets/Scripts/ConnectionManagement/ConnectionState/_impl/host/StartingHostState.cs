#region

#endregion

#region

using System;
using System.Text;
using ConnectionManagement.ConnectionState._impl._common;
using ConnectionManagement.model;
using Gameplay.Config;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace ConnectionManagement.ConnectionState._impl.host
{
    /// <summary>
    /// Connection State when host is starting up.
    /// </summary>
    public class StartingHostState : OnlineState
    {
        ConnectionMethod m_ConnectionMethod;

        public StartingHostState(ConnectionManager connectionManager, ConnectionMethod connectionMethod) : base(
            connectionManager)
        {
            m_ConnectionMethod = connectionMethod;
        }

        public override void Enter()
        {
            StartHost();
        }

        public override void Exit()
        {
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                var payload = Encoding.UTF8.GetString(connectionData);
                var connectionPayload =
                    JsonUtility.FromJson<ConnectionPayload>(
                        payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId,
                    connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, 0, true));

                Debug.Log($"ConnectionManager: Approving self as host");
                // connection approval will create a player object for you
                response.Approved = true;
                //response.CreatePlayerObject = true;
            }
        }

        async void StartHost()
        {
            try
            {
                await m_ConnectionMethod.SetupHostConnectionAsync();
                Debug.Log($"Created relay allocation");

                // NGO's StartHost launches everything
                if (NetworkManager.Singleton.StartHost())
                {
                    SceneTransitionHandler.Instance.RegisterNetworkCallbacks();
                    SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates
                        .Multiplayer_Game_Lobby);
                }
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        void StartHostFailed()
        {
            m_ConnectionManager.ChangeState(new OfflineState(m_ConnectionManager));
        }

        public override void OnServerStarted()
        {
            m_ConnectionManager.ChangeState(new HostingState(m_ConnectionManager));
        }
    }
}