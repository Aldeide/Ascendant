using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Progression
{
    public class FactionTechTree : NetworkBehaviour
    {
        public static FactionTechTree Instance { get; private set; }

        private List<string> m_UnlockedTechs = new List<string>();
        private Dictionary<string, int> m_ResearchInvestments = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsTechUnlocked(string techName)
        {
            return m_UnlockedTechs.Contains(techName);
        }

        public bool InvestTechParts(TechNode node, int count)
        {
            if (NetworkManager.Singleton != null && !IsServer) return false;
            if (node == null || count <= 0) return false;

            if (IsTechUnlocked(node.TechName)) return false;
            if (!node.CanUnlock(m_UnlockedTechs)) return false;

            m_ResearchInvestments.TryGetValue(node.TechName, out int currentInvestment);
            int newInvestment = currentInvestment + count;

            if (newInvestment >= node.CostInTechParts)
            {
                m_ResearchInvestments[node.TechName] = node.CostInTechParts;
                m_UnlockedTechs.Add(node.TechName);
                node.IsUnlocked = true;
                Debug.Log($"[FactionTechTree] Faction UNLOCKED technology: {node.TechName}!");
                return true;
            }
            else
            {
                m_ResearchInvestments[node.TechName] = newInvestment;
                Debug.Log($"[FactionTechTree] Invested {count} Tech Parts in {node.TechName}. Progress: {newInvestment}/{node.CostInTechParts}");
                return false;
            }
        }

        public int GetInvestment(string techName)
        {
            m_ResearchInvestments.TryGetValue(techName, out int val);
            return val;
        }
    }
}
