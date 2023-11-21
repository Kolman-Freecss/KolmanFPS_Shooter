#region

using Gameplay.Config;
using UnityEngine;
using UnityEngine.UI;

#endregion

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
            //TODO: Disconnect from server if connected
            Application.Quit();
        }

        void OnReturnToLobbyButtonClicked()
        {
            //TODO: Disconnect from server if connected
            SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_Lobby, false);
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