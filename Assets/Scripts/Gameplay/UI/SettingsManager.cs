#region

using Gameplay.Config;
using Modules.CacheModule;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Gameplay.UI
{
    public class SettingsManager : MonoBehaviour
    {
        #region Inspector Variables

        [Header("Buttons")] [SerializeField] private Button backButton;


        [Header("Sliders")] [SerializeField] private Slider m_MasterVolumeSlider;

        [SerializeField] private Slider m_MusicVolumeSlider;

        #endregion

        #region InitData

        private void OnEnable()
        {
            backButton.onClick.AddListener(() =>
            {
                SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Home);
            });

            // Note that we initialize the slider BEFORE we listen for changes (so we don't get notified of our own change!)
            m_MasterVolumeSlider.value =
                GameManager.Instance.CacheManagement.GetPlayerCache<float>(PlayerCache.PlayerCacheKeys.MasterVolume);
            m_MasterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);

            // initialize music slider similarly.
            m_MusicVolumeSlider.value =
                GameManager.Instance.CacheManagement.GetPlayerCache<float>(PlayerCache.PlayerCacheKeys.MusicVolume);
            m_MusicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        }

        #endregion

        #region Logic

        private void OnMasterVolumeSliderChanged(float newValue)
        {
            GameManager.Instance.CacheManagement.SavePlayerCache(PlayerCache.PlayerCacheKeys.MasterVolume,
                newValue.ToString());
        }

        private void OnMusicVolumeSliderChanged(float newValue)
        {
            GameManager.Instance.CacheManagement.SavePlayerCache(PlayerCache.PlayerCacheKeys.MusicVolume,
                newValue.ToString());
        }

        #endregion

        #region Destructor

        private void OnDisable()
        {
            backButton.onClick.RemoveAllListeners();
            m_MasterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
            m_MusicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderChanged);
        }

        #endregion
    }
}