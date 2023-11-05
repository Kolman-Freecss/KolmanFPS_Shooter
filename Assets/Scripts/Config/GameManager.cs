using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

namespace Config
{
    public class GameManager : NetworkBehaviour
    {
        #region Auxiliar properties

        public static GameManager Instance { get; private set; }
        
        [HideInInspector]
        public NetworkVariable<bool> isGameStarted = new NetworkVariable<bool>(false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);
        
        [HideInInspector]
        public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false, 
            NetworkVariableReadPermission.Everyone, 
            NetworkVariableWritePermission.Server);

        #endregion

        #region InitData

        void Awake()
        {
            Assert.IsNull(Instance, $"Multiple instances of {nameof(Instance)} detected. This should not happen.");
            ManageSingleton();
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

        #endregion

        #region Logic

        private void StartGame()
        {
            if (!isGameStarted.Value && SceneTransitionHandler.Instance.GetCurrentSceneState().Equals(SceneTransitionHandler.SceneStates.InGame))
            {
                Debug.Log("------------------START GAME------------------");
                isGameStarted.Value = true;
            }
        }

        #endregion
        
    }
}