using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Ascendant
{
    public class UnitframeManager : MonoBehaviour
    {
        public RectTransform healthBar;
        public RectTransform shieldBar;

        private PlayerStatsManager playerStatsManager;

        void Start()
        {
            playerStatsManager = GetComponentInParent<PlayerStatsManager>();
        }

        void Update()
        {
            if (healthBar == null || shieldBar == null)
            {
                return;
            }
            healthBar.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                0.98f * playerStatsManager.currentHealth / playerStatsManager.maxHealth
                );
            shieldBar.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                0.98f * playerStatsManager.currentShield / playerStatsManager.maxShield
                );
        }
    }
}


