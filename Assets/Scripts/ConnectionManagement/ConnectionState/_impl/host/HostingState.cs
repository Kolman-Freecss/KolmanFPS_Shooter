#region

using ConnectionManagement.ConnectionState._impl._common;
using Unity.Multiplayer.Samples.BossRoom;

#endregion

namespace ConnectionManagement.ConnectionState._impl.host
{
    public class HostingState : OnlineState
    {
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
    }
}