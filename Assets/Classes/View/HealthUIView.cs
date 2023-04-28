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
        private Character character;

        private float currentHealth;
        private float currentShield;

        private float healthSize;
        private float shieldSize;
        private float healthReserveSize;

        void Start()
        {
            healthSize = healthBar.rect.width;
            healthReserveSize = healthReserveBar.rect.width;
            shieldSize = shieldBar.rect.width;

            // The UI is activated once the local character has been spawned.
            // As such, it should be available on this script's Start function.
            character = GameManager.Instance.localPlayer.controlledCharacter;
            if (character != null)
            {
                currentHealth = character.GetComponent<Controllers.PlayerStatsController>().GetHealthRatio();
                currentShield = character.GetComponent<Controllers.PlayerStatsController>().GetShieldRatio();
            }
            
            UpdateUI();
        }

        void Update()
        {
            currentHealth = character.GetComponent<Controllers.PlayerStatsController>().GetHealthRatio();
            currentShield = character.GetComponent<Controllers.PlayerStatsController>().GetShieldRatio();
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


