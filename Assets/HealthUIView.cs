using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ascendant;

namespace Ascendant.Views
{
    public class HealthUIView : MonoBehaviour
    {
        [SerializeField]
        private RectTransform healthBar;
        [SerializeField]
        private RectTransform healthReserveBar;
        [SerializeField]
        private RectTransform shieldBar;
        [SerializeField]
        private GameObject localPlayer;

        private float currentHealth;
        private float currentShield;

        private float healthSize;
        private float shieldSize;
        private float healthReserveSize;

        void Awake()
        {
            healthSize = healthBar.rect.width;
            healthReserveSize = healthReserveBar.rect.width;
            shieldSize = shieldBar.rect.width;
            localPlayer = GameManager.Instance.localPlayer;
            currentHealth = localPlayer.GetComponent<Player>().controlledCharacter.gameObject.GetComponent<Controllers.PlayerStatsController>().GetHealthRatio();
            currentShield = localPlayer.GetComponent<Player>().controlledCharacter.gameObject.GetComponent<Controllers.PlayerStatsController>().GetShieldRatio();
            UpdateUI();
        }

        void Update()
        {
            if (localPlayer == null)
            {
                localPlayer = GameManager.Instance.localPlayer;
            }
            if (currentHealth == localPlayer.GetComponent<Player>().controlledCharacter.gameObject.GetComponent<Controllers.PlayerStatsController>().GetHealthRatio()
                && currentShield == localPlayer.GetComponent<Player>().controlledCharacter.gameObject.GetComponent<Controllers.PlayerStatsController>().GetShieldRatio())
            {
                return;
            }
            currentHealth = localPlayer.GetComponent<Player>().controlledCharacter.gameObject.GetComponent<Controllers.PlayerStatsController>().GetHealthRatio();
            currentShield = localPlayer.GetComponent<Player>().controlledCharacter.gameObject.GetComponent<Controllers.PlayerStatsController>().GetShieldRatio();
            UpdateUI();
        }

        private void UpdateUI()
        {
            
            // Health.
            if (currentHealth < 0.2)
            {
                healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0);
                healthReserveBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthReserveSize * currentHealth / 0.2f);
            } else
            {
                healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthSize * (currentHealth * 0.8f + 0.2f));
                healthReserveBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, healthReserveSize);
            }
            // Shields.
            shieldBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentShield * shieldSize);
        }
    }
}


