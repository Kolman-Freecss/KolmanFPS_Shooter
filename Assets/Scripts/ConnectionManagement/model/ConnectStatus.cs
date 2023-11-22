namespace ConnectionManagement.model
{
    public enum ConnectStatus
    {
        Success,
        ServerFull,
        LoggedInAgain,
        UserRequestedDisconnect,
        GenericDisconnect,
        Reconnecting,
        HostEndedSession,
        StartHostFailed,
        StartClientFailed
    }
}