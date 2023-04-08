using Ascendant.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Ascendant.Controllers
{
    [RequireComponent(typeof(EntityStatsModel))]
    public class PlayerStatsController : MonoBehaviour
    {
        public EntityStatsModel statsModel;

        // Start is called before the first frame update
        void Start()
        {
            statsModel = GetComponent<EntityStatsModel>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public Networking.PlayerStatsData ToPlayerStatsData()
        {
            return new Networking.PlayerStatsData(
                GameManager.Instance.localPlayerId,
                statsModel.currentHealth,
                statsModel.maxHealth,
                statsModel.currentShield,
                statsModel.maxShield
                );
        }

        public float GetHealth()
        {
            return statsModel.currentHealth;
        }

        public void RestoreAll()
        {
            statsModel.currentHealth = statsModel.maxHealth;
            statsModel.currentShield = statsModel.maxShield;
        }
        public void Damage(float damage)
        {
            statsModel.currentShield -= damage;
            if (statsModel.currentShield < 0)
            {
                statsModel.currentHealth += statsModel.currentShield;
                statsModel.currentShield = 0;
                if (statsModel.currentHealth < 0)
                {
                    statsModel.currentHealth = 0;
                }
            }
        }
    }
}

