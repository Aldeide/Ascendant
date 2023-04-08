using Ascendant.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.Controllers
{
    [RequireComponent(typeof(EntityStateModel))]
    [RequireComponent(typeof(EntityStatsModel))]
    public class EntityStateSyncController : MonoBehaviour
    {
        public Models.EntityStateModel entityState;
        public Models.EntityStatsModel entityStats;
        void Start()
        {
            entityState = GetComponent<EntityStateModel>();
            entityStats = GetComponent<EntityStatsModel>();
        }

        public void SyncState(Networking.PlayerStateData data)
        {
            entityState.SyncFromNetworkedState(data);
            this.transform.position = data.position;
            this.transform.rotation = data.rotation;
        }

        public void SyncStats(Networking.PlayerStatsData data)
        {
            entityStats.SyncFromNetworkedStats(data);
        }
    }
}



