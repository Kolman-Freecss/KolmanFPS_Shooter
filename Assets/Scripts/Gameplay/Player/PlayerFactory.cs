using Entities.Player;
using Entities.Player.SO;
using Gameplay.GameplayObjects;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Class to manage the creation of players (Skin, name, etc)
    /// </summary>
    public static class PlayerFactory
    {
        
        static readonly string ResourcesPath = "Player/Skins/";
        
        public static Entities.Player.Player CreatePlayer(Entities.Player.Player.PlayerSkin skin, string name)
        {
            Entities.Player.Player player = new Entities.Player.Player();
            player.NameValue = name;
            player.PlayerSkinSOValue = GetPlayerSkin(skin);
            return player;
        }
        
        private static PlayerSkinSO GetPlayerSkin(Entities.Player.Player.PlayerSkin skin)
        {
            return Resources.Load<PlayerSkinSO>(ResourcesPath + skin.ToString());
        }
        
    }
}