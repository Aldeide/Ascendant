using NUnit.Framework;
using UnityEngine;
using Ascendant.SystemsExtensions.Progression;
using Ascendant.SystemsExtensions.Logistics;

namespace Ascendant.Tests
{
    public class RankingTests
    {
        private GameObject m_TrackerObject;
        private GameObject m_ManagerObject;

        [SetUp]
        public void SetUp()
        {
            m_TrackerObject = new GameObject("WarContributionTracker");
            m_TrackerObject.AddComponent<WarContributionTracker>();

            m_ManagerObject = new GameObject("RankManager");
            m_ManagerObject.AddComponent<RankManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (m_TrackerObject != null) Object.DestroyImmediate(m_TrackerObject);
            if (m_ManagerObject != null) Object.DestroyImmediate(m_ManagerObject);
        }

        [Test]
        public void Test_ContributionPointsTracking()
        {
            var tracker = WarContributionTracker.Instance;
            string player = "player_123_abc";

            Assert.AreEqual(0, tracker.GetContributionPoints(player));

            tracker.AddContributionPoints(player, 50);
            Assert.AreEqual(50, tracker.GetContributionPoints(player));

            tracker.AddContributionPoints(player, 120);
            Assert.AreEqual(170, tracker.GetContributionPoints(player));
        }

        [Test]
        public void Test_RankResolutionBranches()
        {
            var manager = RankManager.Instance;

            // Tactical Branch Ranks
            Assert.AreEqual("Ensign", manager.ResolveRank("Tactical", 50));
            Assert.AreEqual("Lieutenant", manager.ResolveRank("Tactical", 250));
            Assert.AreEqual("Commander", manager.ResolveRank("Tactical", 600));
            Assert.AreEqual("Admiral", manager.ResolveRank("Tactical", 1200));

            // Logistics Branch Ranks
            Assert.AreEqual("Quartermaster Apprentice", manager.ResolveRank("Logistics", 50));
            Assert.AreEqual("Transport Officer", manager.ResolveRank("Logistics", 250));
            Assert.AreEqual("Logistics Coordinator", manager.ResolveRank("Logistics", 600));
            Assert.AreEqual("Logistics Director", manager.ResolveRank("Logistics", 1200));

            // Science Branch Ranks
            Assert.AreEqual("Cadet", manager.ResolveRank("Science", 50));
            Assert.AreEqual("Field Technician", manager.ResolveRank("Science", 250));
            Assert.AreEqual("Senior Engineer", manager.ResolveRank("Science", 600));
            Assert.AreEqual("Research Director", manager.ResolveRank("Science", 1200));
        }

        [Test]
        public void Test_DatabaseProfilePersistence()
        {
            using (var conn = DatabaseConnectionManager.CreateConnection("URI=file::memory:"))
            {
                var repo = new WorldDatabaseRepository(conn);
                repo.CreateTables();

                string player = "player_persist_999";
                string branch = "Science";
                int wcp = 750;
                string rank = "Senior Engineer";

                repo.SavePlayerProfile(player, rank, wcp, branch);

                bool exists = repo.LoadPlayerProfile(player, out string loadedRank, out int loadedWcp, out string loadedBranch);
                Assert.IsTrue(exists);
                Assert.AreEqual(rank, loadedRank);
                Assert.AreEqual(wcp, loadedWcp);
                Assert.AreEqual(branch, loadedBranch);
            }
        }
    }
}
