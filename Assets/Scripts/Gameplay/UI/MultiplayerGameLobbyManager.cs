using Config;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI
{
    public class MultiplayerGameLobbyManager : NetworkBehaviour
    {
        #region Inspector Variables

        [Header("Layout Buttons")] [SerializeField]
        private Button serverStartButton;

        [SerializeField] private Button clientReadyButton;

        #endregion

        #region InitData

        private void Awake()
        {
            SubscribeEvents();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                serverStartButton.gameObject.SetActive(true);
                clientReadyButton.gameObject.SetActive(true);
                serverStartButton.onClick.AddListener(OnServerStartButtonClicked);
            }
            else
            {
                serverStartButton.gameObject.SetActive(false);
                clientReadyButton.gameObject.SetActive(true);
            }
        }

        void SubscribeEvents()
        {
            clientReadyButton.onClick.AddListener(OnClientReadyButtonClicked);
        }

        #endregion

        #region Logic

        private void OnServerStartButtonClicked()
        {
            Debug.Log("Server Start Button Clicked");
            SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_InGame);
            //GameManager.Instance.OnStartGameServerRpc();
        }

        private void OnClientReadyButtonClicked()
        {
            Debug.Log("Client Ready Button Clicked");
        }

        #endregion

        #region Destructor

        private void OnDestroy()
        {
            if (IsServer)
            {
                serverStartButton.onClick.RemoveAllListeners();
            }

            clientReadyButton.onClick.RemoveAllListeners();
        }

        #endregion
    }
}