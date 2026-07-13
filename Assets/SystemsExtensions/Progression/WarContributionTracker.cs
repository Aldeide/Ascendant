using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Progression
{
    public class WarContributionTracker : NetworkBehaviour
    {
        public static WarContributionTracker Instance { get; private set; }

        private Dictionary<string, int> m_PlayerWCP = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void AddContributionPoints(string playerGuid, int points)
        {
            if (NetworkManager.Singleton != null && !IsServer) return;
            if (string.IsNullOrEmpty(playerGuid) || points <= 0) return;

            m_PlayerWCP.TryGetValue(playerGuid, out int currentPoints);
            int newPoints = currentPoints + points;
            m_PlayerWCP[playerGuid] = newPoints;

            Debug.Log($"[WarContributionTracker] Player '{playerGuid}' gained {points} WCP. Total WCP: {newPoints}");

            // Automatically check for rank upgrades and persist profile to SQL database
            if (RankManager.Instance != null)
            {
                RankManager.Instance.UpdatePlayerRankAndPersist(playerGuid, newPoints);
            }
        }

        public int GetContributionPoints(string playerGuid)
        {
            m_PlayerWCP.TryGetValue(playerGuid, out int val);
            return val;
        }
    }
}
