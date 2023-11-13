using Entities.Camera;
using Entities.Player.Skin;
using UnityEngine;

namespace Gameplay.Player
{
    /// <summary>
    /// Class to manage the creation of players (typeSkin, name, etc)
    /// </summary>
    public static class PlayerFactory
    {
        
        static readonly string ResourcesPath = "Player/Skins/";
        
        /// <summary>
        /// Create a player with the given parameters and CameraMode to set the correct skin view
        /// </summary>
        /// <param name="cameraMode"></param>
        /// <param name="typeSkin"></param>
        /// <param name="name"></param>
        /// <param name="playerPrefab"></param>
        /// <returns></returns>
        public static Entities.Player.Player CreatePlayer(
            CameraMode cameraMode,
            Entities.Player.Player.PlayerTypeSkin typeSkin, 
            string name,
            GameObject playerPrefab
            )
        {
            Entities.Player.Player player = new Entities.Player.Player();
            player.NameValue = name;
            player.PlayerPrefabValue = playerPrefab ? playerPrefab : GetPlayerSkinPrefab(typeSkin);
            player.PlayerSkinValue = player.PlayerPrefabValue.GetComponentInChildren<PlayerSkin>();
            player.Init(cameraMode);
            return player;
        }
        
        private static GameObject GetPlayerSkinPrefab(Entities.Player.Player.PlayerTypeSkin typeSkin)
        {
            return Resources.Load<GameObject>(ResourcesPath + typeSkin.ToString());
        }
        
    }
}