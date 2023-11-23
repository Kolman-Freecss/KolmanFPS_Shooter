#region

using Gameplay.Config;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class HomeManager : MonoBehaviour
{
    #region Inspector Variables

    [Header("Buttons")] [SerializeField] private Button quitButton;
    [SerializeField] private Button multiplayerButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;

    #endregion


    #region Init Data

    void Start()
    {
        SubscribeToEvents();
    }

    void SubscribeToEvents()
    {
        quitButton.onClick.AddListener(() => { OnQuitButtonClicked(); });
        multiplayerButton.onClick.AddListener(() => { OnMultiplayerButtonClicked(); });
        settingsButton.onClick.AddListener(() => { OnSettingsButtonClicked(); });
        creditsButton.onClick.AddListener(() => { OnCreditsButtonClicked(); });
    }

    #endregion


    #region Logic

    void OnQuitButtonClicked()
    {
        Application.Quit();
    }

    void OnMultiplayerButtonClicked()
    {
        SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Multiplayer_Starting, false);
    }

    void OnSettingsButtonClicked()
    {
        SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Settings, false);
    }

    void OnCreditsButtonClicked()
    {
        SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Credits, false);
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
        multiplayerButton.onClick.RemoveListener(OnMultiplayerButtonClicked);
        settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        creditsButton.onClick.RemoveListener(OnCreditsButtonClicked);
    }

    #endregion
}