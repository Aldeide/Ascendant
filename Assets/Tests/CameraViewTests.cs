using NUnit.Framework;
using UnityEngine;
using Ascendant.SystemsExtensions.Celestial;

namespace Ascendant.Tests
{
    public class CameraViewTests
    {
        private GameObject m_ManagerObj;
        private GameObject m_CameraObj;
        private GameObject m_PlanetObj;

        [SetUp]
        public void SetUp()
        {
            m_ManagerObj = new GameObject("ViewModeManager");
            m_CameraObj = new GameObject("TestCamera");
            m_PlanetObj = new GameObject("TestPlanet");
        }

        [TearDown]
        public void TearDown()
        {
            if (m_ManagerObj != null) Object.DestroyImmediate(m_ManagerObj);
            if (m_CameraObj != null) Object.DestroyImmediate(m_CameraObj);
            if (m_PlanetObj != null) Object.DestroyImmediate(m_PlanetObj);
        }

        [Test]
        public void Test_ZoomThresholdTransition()
        {
            var manager = m_ManagerObj.AddComponent<ViewModeManager>();
            var cam = m_CameraObj.AddComponent<Camera>();
            manager.TargetCamera = cam;
            manager.TacticalZoomThreshold = 80f;

            // Start close
            manager.SetZoom(20f);
            Assert.AreEqual(ViewMode.CloseUp, manager.CurrentMode);

            // Zoom past threshold
            manager.SetZoom(100f);
            Assert.AreEqual(ViewMode.Tactical, manager.CurrentMode);

            // Zoom back close
            manager.SetZoom(50f);
            Assert.AreEqual(ViewMode.CloseUp, manager.CurrentMode);
        }

        [Test]
        public void Test_CameraProjectionSwap()
        {
            var manager = m_ManagerObj.AddComponent<ViewModeManager>();
            var cam = m_CameraObj.AddComponent<Camera>();
            manager.TargetCamera = cam;
            manager.TacticalZoomThreshold = 80f;

            // CloseUp -> Perspective
            manager.SetZoom(20f);
            Assert.IsFalse(cam.orthographic);

            // Tactical -> Orthographic
            manager.SetZoom(100f);
            Assert.IsTrue(cam.orthographic);
            Assert.AreEqual(100f, cam.orthographicSize);
        }

        [Test]
        public void Test_TacticalIconVisibility()
        {
            var manager = m_ManagerObj.AddComponent<ViewModeManager>();
            var cam = m_CameraObj.AddComponent<Camera>();
            manager.TargetCamera = cam;
            manager.TacticalZoomThreshold = 80f;

            // Setup planet with 3D child visual and TacticalIcon
            var body = m_PlanetObj.AddComponent<CelestialBody>();
            body.Radius = 10f;
            body.Type = CelestialType.TerrestrialPlanet;
            body.CreateVisualRepresentation(Color.blue);

            var icon = m_PlanetObj.AddComponent<TacticalIcon>();
            icon.TacticalColor = Color.cyan;
            icon.InitializeVisuals();

            // At start (CloseUp zoom), 3D visual is active, tactical visual is inactive
            manager.SetZoom(20f);
            manager.NotifyViewModeChanged();

            Assert.IsTrue(icon.Visual3D.activeSelf);
            Assert.IsFalse(icon.VisualTactical.activeSelf);

            // Zoom to Tactical, 3D visual becomes inactive, tactical visual becomes active
            manager.SetZoom(100f);
            manager.NotifyViewModeChanged();

            Assert.IsFalse(icon.Visual3D.activeSelf);
            Assert.IsTrue(icon.VisualTactical.activeSelf);
        }
    }
}
