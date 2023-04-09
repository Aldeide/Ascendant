using Ascendant.Networking;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Ascendant.Models
{
    public class EntityStatsModel : NetworkBehaviour
    {
        [SyncVar]
        public float currentHealth = 100.0f;
        [SyncVar]
        public float maxHealth = 100.0f;
        [SyncVar]
        public float currentShield = 100.0f;
        [SyncVar]
        public float maxShield = 100.0f;

        internal void SyncFromNetworkedStats(PlayerStatsData data)
        {
            this.currentHealth = data.currentHealth;
            this.currentShield = data.currentShield;
            this.maxHealth = data.maxHealth;
            this.maxShield = data.maxShield;
        }
    }
}

