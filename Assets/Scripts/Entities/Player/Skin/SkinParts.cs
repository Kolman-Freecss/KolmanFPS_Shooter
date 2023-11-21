#region

using System;
using UnityEngine;

#endregion

namespace Entities.Player.Skin
{
    /// <summary>
    /// Positions of the player's body parts
    /// </summary>
    [Serializable]
    public class SkinParts
    {
        [SerializeField] private Transform m_rightHand;
        public Transform RightHand => m_rightHand;

        [SerializeField] private Transform m_leftHand;
        public Transform LeftHand => m_leftHand;

        [SerializeField] private Transform m_head;
        public Transform Head => m_head;
    }
}