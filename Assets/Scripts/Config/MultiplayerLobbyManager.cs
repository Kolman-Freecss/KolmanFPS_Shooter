using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Config
{
    public class MultiplayerLobbyManager : MonoBehaviour
    {
        #region Inspector Variables

        [SerializeField]
        private Button startHostButton;
        
        [SerializeField]
        private Button startClientButton;

        #endregion

        #region InitData

        private void Awake()
        {
            SubscribeEvents();
        }
        
        private void SubscribeEvents()
        {
            startHostButton.onClick.AddListener(() =>
            {
                ConnectionManager.Instance.StartHost();
            });
            startClientButton.onClick.AddListener(() =>
            {
                ConnectionManager.Instance.StartClient();
            });
        }

        #endregion

        #region Logic

        

        #endregion
        
    }
}