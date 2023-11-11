using System;
using TMPro;
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
            Int32.TryParse(_textHostPort.text, out var portInt);
            if (portInt <= 0)
            {
                portInt = DefaultPort;
            }

            ConnectionManager.Instance.StartHost(DefaultIp, portInt);
        }
        
        void OnStartGameClientButton()
        {
            Int32.TryParse(_textClientPortToConnect.text, out var portInt);
            if (portInt <= 0)
            {
                portInt = DefaultPort;
            }
            
            string ipAddress = string.IsNullOrEmpty(_textClientIPTToConnect.text) ? DefaultIp : _textClientIPTToConnect.text;
            
            ConnectionManager.Instance.StartClient(ipAddress, portInt);
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