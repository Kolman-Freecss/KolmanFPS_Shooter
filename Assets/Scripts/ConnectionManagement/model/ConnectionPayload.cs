#region

using System;

#endregion

namespace ConnectionManagement.model
{
    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
        public string playerName;
        public bool isDebug;
    }
}