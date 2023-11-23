#region

using Gameplay.Config;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

namespace Gameplay.UI
{
    public class CreditsManager : MonoBehaviour
    {
        #region Inspector Variables

        [FormerlySerializedAs("quitButton")] [Header("Buttons")] [SerializeField]
        private Button backButton;

        #endregion

        #region Init Data

        void Start()
        {
            SubscribeToEvents();
        }

        void SubscribeToEvents()
        {
            backButton.onClick.AddListener(() => { OnBackButtonClicked(); });
        }

        #endregion

        #region Logic

        void OnBackButtonClicked()
        {
            SceneTransitionHandler.Instance.LoadScene(SceneTransitionHandler.SceneStates.Home, false);
        }

        #endregion

        #region Destructor

        private void OnDestroy()
        {
            UnsubscribeToEvents();
        }

        void UnsubscribeToEvents()
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }

        #endregion
    }
}