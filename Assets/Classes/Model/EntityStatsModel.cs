using Ascendant.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Models
{
    public class EntityStatsModel : MonoBehaviour
    {
        public float currentHealth;
        public float maxHealth;

        public float currentShield;
        public float maxShield;

        public EntityStatsModel()
        {
            maxHealth = 100;
            currentHealth = 100;
            maxShield = 100;
            currentShield = 100;
        }

        internal void SyncFromNetworkedStats(PlayerStatsData data)
        {
            this.currentHealth = data.currentHealth;
            this.currentShield = data.currentShield;
            this.maxHealth = data.maxHealth;
            this.maxShield = data.maxShield;
        }
    }
}

