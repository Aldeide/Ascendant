using Ascendant.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Ascendant.Controllers
{
    [RequireComponent(typeof(EntityStatsModel))]
    public class PlayerStatsController : NetworkBehaviour
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

        public float GetHealth()
        {
            return statsModel.currentHealth;
        }

        public float GetHealthRatio()
        {
            return statsModel.currentHealth / statsModel.maxHealth;
        }

        public float GetShieldRatio()
        {
            return statsModel.currentShield / statsModel.maxShield;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RestoreAll()
        {
            statsModel.currentHealth = statsModel.maxHealth;
            statsModel.currentShield = statsModel.maxShield;
        }
        [ServerRpc(RequireOwnership = false)]
        public void Damage(float damage)
        {
            Debug.Log("Applying damage: " + damage);
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

