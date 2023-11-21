#region

using System.Numerics;
using Entities.Player;
using Unity.Multiplayer.Samples.BossRoom;

#endregion

namespace ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;
        public Player.TeamType TeamType;

        public SessionPlayerData(ulong clientID, string name, int currentHitPoints = 0, bool isConnected = false,
            bool hasCharacterSpawned = false, Player.TeamType teamType = Player.TeamType.None)
        {
            ClientID = clientID;
            PlayerName = name;
            TeamType = teamType;
            PlayerNumber = -1;
            PlayerPosition = Vector3.Zero;
            PlayerRotation = Quaternion.Identity;
            CurrentHitPoints = currentHitPoints;
            IsConnected = isConnected;
            HasCharacterSpawned = hasCharacterSpawned;
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public void Reinitialize()
        {
            HasCharacterSpawned = false;
        }
    }
}