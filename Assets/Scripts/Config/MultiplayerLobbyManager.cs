using System;
using System.Text.RegularExpressions;
using ConnectionManagement;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace Config
{
    public class MultiplayerLobbyManager : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Multiplayer Layout")]
        [SerializeField]
        private Button startHostButton;
        
        [SerializeField]
        private Button startClientButton;
        
        [Header("StartHost Layout")]
        [SerializeField]
        private Button startGameHostButton;
        [SerializeField]
        private TextMeshProUGUI _textHostPort;
        
        [Header("StartClient Layout")]
        [SerializeField]
        private Button startGameClientButton;
        [SerializeField]
        private TextMeshProUGUI _textClientIPTToConnect;
        [SerializeField]
        private TextMeshProUGUI _textClientPortToConnect;
        
        
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
            
            if (_textClientIPTToConnect == null) _textClientIPTToConnect = GameObject.Find("TextClientIPTToConnect").GetComponent<TextMeshProUGUI>();
            if (_textClientPortToConnect == null) _textClientPortToConnect = GameObject.Find("TextClientPortToConnect").GetComponent<TextMeshProUGUI>();
            if (_textHostPort == null) _textHostPort = GameObject.Find("TextHostPort").GetComponent<TextMeshProUGUI>();
        }

        void Init()
        {
            _verticalLayoutStartHostGame.SetActive(false);
            _verticalLayoutStartClientGame.SetActive(false);
        }

        private void SubscribeEvents()
        {
            startHostButton.onClick.AddListener(() =>
            {
                OnStartHostButton();
            });
            startClientButton.onClick.AddListener(() =>
            {
                OnStartClientButton();
            });
            startGameHostButton.onClick.AddListener(() =>
            {
                OnStartGameHostButton();
            });
            startGameClientButton.onClick.AddListener(() =>
            {
                OnStartGameClientButton();
            });
        }

        #endregion

        #region Logic
        
        /// <summary>
        /// The purpose of this method is to set the player prefab index to be used by the client when connecting to the server
        /// This will be handled by the ConnectionApprovalCallback in the ConnectionManager
        /// @see ConnectionManager
        /// </summary>
        /// <param name="index"></param>
        public void SetClientPlayerPrefab(Entities.Player.Player.TeamType teamType)
        {
            uint globalNetworkId = GameManager.Instance.SkinsGlobalNetworkIds[teamType];
            if (globalNetworkId > GameManager.Instance.SkinsGlobalNetworkIds.Count)
            {
                Debug.LogError($"Trying to assign player Prefab index of {globalNetworkId} when there are only {GameManager.Instance.SkinsGlobalNetworkIds.Count} entries!");
                return;
            }
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogError("This needs to be set this before connecting!");
                return;
            }
            NetworkManager.Singleton.NetworkConfig.ConnectionData = System.BitConverter.GetBytes(teamType.GetHashCode());
        }

        void OnStartClientButton()
        {
            _verticalLayoutMultiplayer.SetActive(false);
            _verticalLayoutStartClientGame.SetActive(true);
        }
        
        void OnStartHostButton()
        {
            _verticalLayoutMultiplayer.SetActive(false);
            _verticalLayoutStartHostGame.SetActive(true);
        }
        
        void OnStartGameHostButton()
        {
            SetClientPlayerPrefab(Entities.Player.Player.TeamType.Warriors);
            Int32.TryParse(_textHostPort.text, out var portInt);
            if (portInt <= 0)
            {
                portInt = DefaultPort;
            }

            if (!CheckRegex(portInt.ToString(), RegexPort))
            {
                portInt = DefaultPort;
            }

            ConnectionManager.Instance.StartHost(DefaultIp, portInt);
        }
        
        void OnStartGameClientButton()
        {
            SetClientPlayerPrefab(Entities.Player.Player.TeamType.Wizards);
            Int32.TryParse(_textClientPortToConnect.text, out var portInt);
            if (portInt <= 0)
            {
                portInt = DefaultPort;
            }

            if (!CheckRegex(portInt.ToString(), RegexPort))
            {
                //TODO: Notify to client that the port is not valid and we are using the default port
                portInt = DefaultPort;
            }
            
            string ipAddress = string.IsNullOrEmpty(_textClientIPTToConnect.text) ? DefaultIp : _textClientIPTToConnect.text;

            if (!CheckRegex(_textClientIPTToConnect.text, RegexIp))
            {
                //TODO: Notify to client that the ip is not valid and we are using the default ip
                ipAddress = DefaultIp;
            }
            
            ConnectionManager.Instance.StartClient(ipAddress, portInt);
        }

        private bool CheckRegex(string input, string regex)
        {
            bool isValid = false;
            if (Regex.IsMatch(input, regex))
            {
                isValid = true;
            }
            else
            {
                Debug.LogWarning("IP is not valid");
                //TODO: Show message to the client
            }

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