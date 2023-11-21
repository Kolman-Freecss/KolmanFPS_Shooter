#region

using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Config
{
    public class SceneTransitionHandler : NetworkBehaviour
    {
        #region Inspector Variables

        [SerializeField] public SceneStates DefaultScene = SceneStates.Multiplayer_Lobby;

        #endregion

        #region Auxiliar properties

        public static SceneTransitionHandler Instance { get; private set; }

        private SceneStates m_SceneState;

        #endregion

        #region Event Delegates

        [HideInInspector]
        public delegate void SceneStateChangedDelegateHandler(SceneStates newState);

        [HideInInspector] public event SceneStateChangedDelegateHandler OnSceneStateChanged;

        [HideInInspector]
        public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);

        [HideInInspector] public event ClientLoadedSceneDelegateHandler OnClientLoadedGameScene;

        #endregion

        public enum SceneStates
        {
            Init,
            Multiplayer_Lobby,
            Multiplayer_Game_Lobby,
            Multiplayer_InGame,
            Multiplayer_EndGame,
        }

        #region InitData

        void Awake()
        {
            ManageSingleton();
            SetSceneState(SceneStates.Init);
        }

        private void ManageSingleton()
        {
            if (Instance != null)
            {
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }

        void Start()
        {
            if (m_SceneState == SceneStates.Init)
            {
                LoadScene(DefaultScene);
            }
        }

        #endregion

        #region Logic

        public void SetSceneState(SceneStates sceneState)
        {
            m_SceneState = sceneState;
            if (OnSceneStateChanged != null)
            {
                OnSceneStateChanged.Invoke(m_SceneState);
            }

            if (sceneState == SceneStates.Multiplayer_InGame)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        /// <summary>
        /// Load a scene for all clients or only for a single client
        /// </summary>
        /// <param name="sceneState"></param>
        /// <param name="loadForAll"></param>
        public void LoadScene(SceneStates sceneState, bool loadForAll = true)
        {
            if (NetworkManager.Singleton.IsListening && loadForAll)
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneState.ToString(), LoadSceneMode.Single);
            }
            else
            {
                SceneManager.LoadSceneAsync(sceneState.ToString());
            }

            SetSceneState(sceneState);
        }

        #endregion

        #region Network Calls/Events

        public void RegisterNetworkCallbacks()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
        }

        private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log("OnLoadComplete " + sceneName);
            if (SceneStates.Multiplayer_InGame.ToString().Equals(sceneName))
            {
                OnClientLoadedGameScene?.Invoke(clientId);
            }
        }

        #endregion

        #region Destructor

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            UnRegisterNetworkCallbacks();
        }

        private void UnRegisterNetworkCallbacks()
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnLoadComplete;
        }

        #endregion

        #region Getter & Setter

        public SceneStates GetCurrentSceneState()
        {
            return m_SceneState;
        }

        #endregion
    }
}