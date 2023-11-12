using Config;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class MultiplayerEndGame : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Buttons")] [SerializeField] Button quitButton;
        [SerializeField] Button returnToLobbyButton;

        #endregion

        #region Init Data

        void Start()
        {
            SubscribeToEvents();
        }

        void SubscribeToEvents()
        {
            quitButton.onClick.AddListener(() => { OnQuitButtonClicked(); });
            returnToLobbyButton.onClick.AddListener(() => { OnReturnToLobbyButtonClicked(); });
        }

        #endregion

        #region Logic

        void OnQuitButtonClicked()
        {
            ConnectionManager.Instance.Disconnect(NetworkManager.Singleton.LocalClientId);
            Application.Quit();
        }

        void OnReturnToLobbyButtonClicked()
        {
            ConnectionManager.Instance.Disconnect(NetworkManager.Singleton.LocalClientId);
            SceneTransitionHandler.Instance.SetSceneState(SceneTransitionHandler.SceneStates.Multiplayer_Lobby);
        }

        #endregion

        #region Destructor

        private void OnDestroy()
        {
            UnsubscribeToEvents();
        }

        void UnsubscribeToEvents()
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
            returnToLobbyButton.onClick.RemoveListener(OnReturnToLobbyButtonClicked);
        }

        #endregion
    }
}