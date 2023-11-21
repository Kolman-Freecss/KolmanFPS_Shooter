#region

using System.Text.RegularExpressions;
using ConnectionManagement;
using Gameplay.Config;
using Modules.CacheModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Gameplay.UI
{
    public class MultiplayerLobbyManager : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Multiplayer Layout")] [SerializeField]
        private Button startHostButton;

        [SerializeField] private Button startClientButton;

        [Header("StartHost Layout")] [SerializeField]
        private Button startGameHostButton;

        [SerializeField] private TextMeshProUGUI _textHostPort;

        [Header("StartClient Layout")] [SerializeField]
        private Button startGameClientButton;

        [SerializeField] private TextMeshProUGUI _textClientIPTToConnect;

        [SerializeField] private TextMeshProUGUI _textClientPortToConnect;

        #endregion

        #region Member Variables

        private const string RegexIp = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
        private const string RegexPort = @"^[0-9]{1,5}$";
        public const string DefaultIp = "127.0.0.1";
        public const int DefaultPort = 7777;

        private GameObject _verticalLayoutMultiplayer;
        private GameObject _verticalLayoutStartHostGame;
        private GameObject _verticalLayoutStartClientGame;

        #endregion

        #region InitData

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void Start()
        {
            GetReferences();
            Init();
        }

        private void GetReferences()
        {
            _verticalLayoutMultiplayer = GameObject.Find("VerticalLayoutMultiplayer");
            _verticalLayoutStartHostGame = GameObject.Find("VerticalLayoutStartHostGame");
            _verticalLayoutStartClientGame = GameObject.Find("VerticalLayoutStartClientGame");

            if (_textClientIPTToConnect == null)
                _textClientIPTToConnect = GameObject.Find("TextClientIPTToConnect").GetComponent<TextMeshProUGUI>();
            if (_textClientPortToConnect == null)
                _textClientPortToConnect = GameObject.Find("TextClientPortToConnect").GetComponent<TextMeshProUGUI>();
            if (_textHostPort == null) _textHostPort = GameObject.Find("TextHostPort").GetComponent<TextMeshProUGUI>();
        }

        private void Init()
        {
            _verticalLayoutStartHostGame.SetActive(false);
            _verticalLayoutStartClientGame.SetActive(false);
        }

        private void SubscribeEvents()
        {
            startHostButton.onClick.AddListener(() => { OnStartHostButton(); });
            startClientButton.onClick.AddListener(() => { OnStartClientButton(); });
            startGameHostButton.onClick.AddListener(() => { OnStartGameHostButton(); });
            startGameClientButton.onClick.AddListener(() => { OnStartGameClientButton(); });
        }

        #endregion

        #region Logic

        /// <summary>
        ///     The purpose of this method is to set the player prefab index to be used by the client when connecting to the server
        ///     This will be handled by the ConnectionApprovalCallback in the ConnectionManager
        ///     @see ConnectionManager
        /// </summary>
        /// <param name="index"></param>
        public void SetClientPlayerPrefab(Entities.Player.Player.TeamType teamType)
        {
            //TODO: Implement through the ConnectionManager
            // uint globalNetworkId = GameManager.Instance.SkinsGlobalNetworkIds.Find(skin => skin.Key == teamType).Value;
            // if (NetworkManager.Singleton.IsListening)
            // {
            //     Debug.LogError("This needs to be set this before connecting!");
            //     return;
            // }
            // NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(teamType.GetHashCode());
        }

        private void OnStartClientButton()
        {
            _verticalLayoutMultiplayer.SetActive(false);
            _verticalLayoutStartClientGame.SetActive(true);
        }

        private void OnStartHostButton()
        {
            _verticalLayoutMultiplayer.SetActive(false);
            _verticalLayoutStartHostGame.SetActive(true);
        }

        private void OnStartGameHostButton()
        {
            SetClientPlayerPrefab(Entities.Player.Player.TeamType.Wizards);
            int.TryParse(_textHostPort.text, out var portInt);
            if (portInt <= 0) portInt = DefaultPort;

            if (!CheckRegex(portInt.ToString(), RegexPort)) portInt = DefaultPort;

            GameManager.Instance.CacheManagement.SavePlayerCache(PlayerCache.PlayerCacheKeys.Username,
                "DefaultNameHostCached");
            GameManager.Instance.CacheManagement.SavePlayerCache(PlayerCache.PlayerCacheKeys.TeamType,
                Entities.Player.Player.TeamType.Wizards.ToString());

            ConnectionManager.Instance.StartHost(DefaultIp, portInt);
        }

        private void OnStartGameClientButton()
        {
            SetClientPlayerPrefab(Entities.Player.Player.TeamType.Warriors);
            int.TryParse(_textClientPortToConnect.text, out var portInt);
            if (portInt <= 0) portInt = DefaultPort;

            if (!CheckRegex(portInt.ToString(), RegexPort))
                //TODO: Notify to client that the port is not valid and we are using the default port
                portInt = DefaultPort;

            var ipAddress = string.IsNullOrEmpty(_textClientIPTToConnect.text)
                ? DefaultIp
                : _textClientIPTToConnect.text;

            if (!CheckRegex(_textClientIPTToConnect.text, RegexIp))
                //TODO: Notify to client that the ip is not valid and we are using the default ip
                ipAddress = DefaultIp;

            GameManager.Instance.CacheManagement.SavePlayerCache(PlayerCache.PlayerCacheKeys.Username,
                "DefaultNameClientCached");
            GameManager.Instance.CacheManagement.SavePlayerCache(PlayerCache.PlayerCacheKeys.TeamType,
                Entities.Player.Player.TeamType.Warriors.ToString());

            ConnectionManager.Instance.StartClient(ipAddress, portInt);
        }

        private bool CheckRegex(string input, string regex)
        {
            var isValid = false;
            if (Regex.IsMatch(input, regex))
                isValid = true;
            else
                Debug.LogWarning("IP is not valid");
            //TODO: Show message to the client
            return isValid;
        }

        #endregion

        #region Destructor

        public void OnDisable()
        {
            startHostButton.onClick.RemoveAllListeners();
            startClientButton.onClick.RemoveAllListeners();
            startGameHostButton.onClick.RemoveAllListeners();
            startGameClientButton.onClick.RemoveAllListeners();
        }

        #endregion
    }
}