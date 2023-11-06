using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Config
{
    public class SceneTransitionHandler : NetworkBehaviour
    {

        #region Inspector Variables

        [SerializeField]
        public string DefaultScene = "MultiplayerLobby";

        #endregion
        
        #region Auxiliar properties

        public static SceneTransitionHandler Instance { get; private set; }
        
        private SceneStates m_SceneState;

        #endregion

        #region Event Delegates
        
        [HideInInspector]
        public delegate void SceneStateChangedDelegateHandler(SceneStates newState);

        [HideInInspector]
        public event SceneStateChangedDelegateHandler OnSceneStateChanged;

        #endregion
        
        public enum SceneStates
        {
            Init,
            MultiplayerLobby,
            InGame,
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
                LoadScene(SceneStates.MultiplayerLobby);
            }
        }

        #endregion
        
        #region Logic
        
        public void SetSceneState(SceneStates sceneState)
        {
            m_SceneState = sceneState;
            if(OnSceneStateChanged != null)
            {
                OnSceneStateChanged.Invoke(m_SceneState);
            }
        }
        
        public void LoadScene(SceneStates sceneState)
        {
            if (NetworkManager.Singleton.IsListening)
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

        #region Getter & Setter
        
        public SceneStates GetCurrentSceneState()
        {
            return m_SceneState;
        }

        #endregion

    }
}