using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Config
{
    public class SceneTransitionHandler : NetworkBehaviour
    {

        #region Inspector Variables

        [SerializeField]
        public string DefaultScene = "InGame";

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
                SceneManager.LoadScene(DefaultScene);
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
        
        #endregion

        #region Getter & Setter
        
        public SceneStates GetCurrentSceneState()
        {
            return m_SceneState;
        }

        #endregion

    }
}