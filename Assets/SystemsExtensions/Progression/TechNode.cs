using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Progression
{
    [CreateAssetMenu(menuName = "Ascendant/Progression/TechNode")]
    public class TechNode : ScriptableObject
    {
        public string TechName;
        public List<TechNode> Prerequisites = new List<TechNode>();
        public int CostInTechParts = 50;
        public bool IsUnlocked;

        public bool CanUnlock(List<string> unlockedTechs)
        {
            foreach (var pre in Prerequisites)
            {
                if (pre != null && !unlockedTechs.Contains(pre.TechName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
