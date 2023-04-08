using Ascendant.Models;
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

        private EntityStatsModel statsModel;

        void Start()
        {
            statsModel = GetComponentInParent<EntityStatsModel>();
        }

        void Update()
        {
            if (healthBar == null || shieldBar == null)
            {
                return;
            }
            healthBar.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                0.98f * statsModel.currentHealth / statsModel.maxHealth
                );
            shieldBar.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                0.98f * statsModel.currentShield / statsModel.maxShield);
                
        }
    }
}


