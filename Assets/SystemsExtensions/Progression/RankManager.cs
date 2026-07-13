using System;
using System.Collections.Generic;
using Ascendant.SystemsExtensions.Logistics;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Progression
{
    public class RankManager : MonoBehaviour
    {
        public static RankManager Instance { get; private set; }

        private Dictionary<string, string> m_PlayerBranches = new Dictionary<string, string>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetPlayerBranch(string playerGuid, string branch)
        {
            m_PlayerBranches[playerGuid] = branch;
        }

        public string GetPlayerBranch(string playerGuid)
        {
            if (m_PlayerBranches.TryGetValue(playerGuid, out string branch))
            {
                return branch;
            }
            return "Tactical"; // Default branch
        }

        public string ResolveRank(string branch, int wcp)
        {
            switch (branch.ToLowerInvariant())
            {
                case "logistics":
                    if (wcp >= 1000) return "Logistics Director";
                    if (wcp >= 500) return "Logistics Coordinator";
                    if (wcp >= 100) return "Transport Officer";
                    return "Quartermaster Apprentice";

                case "science":
                    if (wcp >= 1000) return "Research Director";
                    if (wcp >= 500) return "Senior Engineer";
                    if (wcp >= 100) return "Field Technician";
                    return "Cadet";

                case "tactical":
                default:
                    if (wcp >= 1000) return "Admiral";
                    if (wcp >= 500) return "Commander";
                    if (wcp >= 100) return "Lieutenant";
                    return "Ensign";
            }
        }

        public void UpdatePlayerRankAndPersist(string playerGuid, int wcp)
        {
            string branch = GetPlayerBranch(playerGuid);
            string rank = ResolveRank(branch, wcp);

            Debug.Log($"[RankManager] Resolved Player '{playerGuid}' Rank: {rank} ({branch})");

            // Persist to the database
            var dbPath = DatabaseConnectionManager.DefaultDbPath;
            try
            {
                using (var conn = DatabaseConnectionManager.CreateConnection(dbPath))
                {
                    var repo = new WorldDatabaseRepository(conn);
                    repo.CreateTables();
                    repo.SavePlayerProfile(playerGuid, rank, wcp, branch);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[RankManager] Error persisting player profile: {ex.Message}");
            }
        }
    }
}
