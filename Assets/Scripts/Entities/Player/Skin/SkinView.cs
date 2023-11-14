using System;
using UnityEngine;

namespace Entities.Player.Skin
{
    /// <summary>
    /// Different skins for the player taking into account the body parts and the Camera mode
    /// </summary>
    [Serializable]
    public class SkinView
    {
        [SerializeField]
        private GameObject skinModel;
        public GameObject SkinModel => skinModel;
        
        [SerializeField]
        private SkinParts _skinParts;
        public SkinParts SkinParts => _skinParts;
    }
}