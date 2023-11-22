#region

using System.Collections;
using System.Text;
using ConnectionManagement.ConnectionState._impl._common;
using ConnectionManagement.model;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace ConnectionManagement.ConnectionState._impl.host
{
    public class HostingState : OnlineState
    {
        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        const int k_MaxConnectPayload = 1024;

        public HostingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                m_ConnectionManager.ChangeState(new OfflineState(m_ConnectionManager));
            }
            else
            {
                var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
                if (playerId != null)
                {
                    var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
                    SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
                }
            }
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
        /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
        /// </summary>
        /// <remarks>
        /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
        /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
        /// when connection is refused, for example).
        /// Note on authentication: It's usually harder to justify having authentication in a client hosted game's connection approval. Since the host can't be trusted,
        /// clients shouldn't send it private authentication tokens you'd usually send to a dedicated server.
        /// </remarks>
        /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
        /// which is a unique identifier for their install of the game that persists across app restarts.
        ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            Debug.Log("ApprovalCheck Hosting State");
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            if (connectionData.Length > k_MaxConnectPayload)
            {
                // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            var payload = Encoding.UTF8.GetString(connectionData);
            var connectionPayload =
                JsonUtility.FromJson<ConnectionPayload>(
                    payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId,
                    connectionPayload.playerId,
                    new SessionPlayerData(clientId, connectionPayload.playerName, 0, true));

                // connection approval will create a player object for you
                response.Approved = true;
                //response.CreatePlayerObject = true;
                // response.Position = Vector3.zero;
                // response.Rotation = Quaternion.identity;
                return;
            }

            // In order for clients to not just get disconnected with no feedback, the server needs to tell the client why it disconnected it.
            // This could happen after an auth check on a service or because of gameplay reasons (server full, wrong build version, etc)
            // Since network objects haven't synced yet (still in the approval process), we need to send a custom message to clients, wait for
            // UTP to update a frame and flush that message, then give our response to NetworkManager's connection approval process, with a denied approval.
            IEnumerator WaitToDenyApproval()
            {
                response.Pending = true; // give some time for server to send connection status message to clients
                response.Approved = false;
                yield return null; // wait a frame so UTP can flush it's messages on next update
                response.Pending = false; // connection approval process can be finished.
            }

            m_ConnectionManager.StartCoroutine(WaitToDenyApproval());
        }

        ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (NetworkManager.Singleton.ConnectedClientsIds.Count >= m_ConnectionManager.MaxPlayers)
            {
                return ConnectStatus.ServerFull;
            }

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId)
                ? ConnectStatus.LoggedInAgain
                : ConnectStatus.Success;
        }

        IEnumerator WaitToShutdown()
        {
            yield return null;
            m_ConnectionManager.ChangeState(new OfflineState(m_ConnectionManager));
        }
    }
}