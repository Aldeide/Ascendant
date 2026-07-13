using System.Collections.Generic;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    public class StarSystem : MonoBehaviour
    {
        [SerializeField] private string m_SystemName;
        
        private CelestialBody m_Star;
        private List<CelestialBody> m_Bodies = new List<CelestialBody>();

        public string SystemName
        {
            get => m_SystemName;
            set => m_SystemName = value;
        }

        public CelestialBody Star => m_Star;
        public List<CelestialBody> Bodies => m_Bodies;

        public void InitializeSystem(string systemName)
        {
            m_SystemName = systemName;

            // Clear old bodies
            foreach (var b in m_Bodies)
            {
                if (b != null) DestroyImmediate(b.gameObject);
            }
            m_Bodies.Clear();
            if (m_Star != null) DestroyImmediate(m_Star.gameObject);

            // 1. Central Star
            var starObj = new GameObject($"{systemName}_Star");
            starObj.transform.SetParent(transform);
            m_Star = starObj.AddComponent<CelestialBody>();
            m_Star.BodyName = systemName == "SystemAlpha" ? "Helios" : "Kepler";
            m_Star.Type = CelestialType.Star;
            m_Star.Radius = 50f;
            m_Star.CreateVisualRepresentation(systemName == "SystemAlpha" ? new Color(1f, 0.9f, 0.3f) : new Color(1f, 0.3f, 0.1f));

            // 2. Terrestrial Planet
            var planetObj = new GameObject($"{systemName}_Planet1");
            planetObj.transform.SetParent(transform);
            planetObj.transform.localPosition = new Vector3(180f, 0f, 0f);
            var planet = planetObj.AddComponent<CelestialBody>();
            planet.BodyName = systemName == "SystemAlpha" ? "Gaia" : "Kepler-b";
            planet.Type = CelestialType.TerrestrialPlanet;
            planet.Radius = 10f;
            planet.ParentBody = m_Star;
            planet.CreateVisualRepresentation(systemName == "SystemAlpha" ? new Color(0.2f, 0.6f, 1f) : new Color(0.1f, 0.8f, 0.7f));
            m_Bodies.Add(planet);

            // 3. Moon orbiting Terrestrial Planet (placed outside planet's 10f radius)
            var moonObj = new GameObject($"{systemName}_Moon");
            moonObj.transform.SetParent(planetObj.transform);
            moonObj.transform.localPosition = new Vector3(25f, 0f, 0f);
            var moon = moonObj.AddComponent<CelestialBody>();
            moon.BodyName = systemName == "SystemAlpha" ? "Luna" : "Kepler-b-I";
            moon.Type = CelestialType.Moon;
            moon.Radius = 3f;
            moon.ParentBody = planet;
            moon.CreateVisualRepresentation(systemName == "SystemAlpha" ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.5f, 0.5f, 0.5f));
            m_Bodies.Add(moon);

            // 4. Gaseous Fuel Planet
            var gasPlanetObj = new GameObject($"{systemName}_Planet2");
            gasPlanetObj.transform.SetParent(transform);
            gasPlanetObj.transform.localPosition = new Vector3(-300f, 0f, 0f);
            var gasPlanet = gasPlanetObj.AddComponent<CelestialBody>();
            gasPlanet.BodyName = systemName == "SystemAlpha" ? "Ares" : "Kepler-c";
            gasPlanet.Type = CelestialType.GaseousPlanet;
            gasPlanet.Radius = 18f;
            gasPlanet.ParentBody = m_Star;
            gasPlanet.CreateVisualRepresentation(systemName == "SystemAlpha" ? new Color(0.9f, 0.5f, 0.2f) : new Color(0.6f, 0.2f, 0.8f));
            m_Bodies.Add(gasPlanet);

            // 5. Asteroid Belt
            var beltObj = new GameObject($"{systemName}_AsteroidBelt");
            beltObj.transform.SetParent(transform);
            var beltBody = beltObj.AddComponent<CelestialBody>();
            beltBody.BodyName = systemName == "SystemAlpha" ? "Prime Belt" : "Kuiper Ring";
            beltBody.Type = CelestialType.AsteroidBelt;
            beltBody.Radius = 450f;
            beltBody.ParentBody = m_Star;
            m_Bodies.Add(beltBody);

            var belt = beltObj.AddComponent<AsteroidBelt>();
            belt.MaxAsteroids = 8;
            belt.InnerRadius = 400f;
            belt.OuterRadius = 500f;
            belt.SpawnAsteroids();
        }
    }
}
