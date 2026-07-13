using System.Collections.Generic;
using Ascendant.SystemsExtensions.Logistics;
using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    [RequireComponent(typeof(CelestialBody))]
    public class AsteroidBelt : MonoBehaviour
    {
        [SerializeField] private int m_MaxAsteroids = 5;
        [SerializeField] private float m_InnerRadius = 200f;
        [SerializeField] private float m_OuterRadius = 300f;

        private List<GameObject> m_SpawnedAsteroids = new List<GameObject>();

        public List<GameObject> SpawnedAsteroids => m_SpawnedAsteroids;

        public int MaxAsteroids
        {
            get => m_MaxAsteroids;
            set => m_MaxAsteroids = value;
        }

        public float InnerRadius
        {
            get => m_InnerRadius;
            set => m_InnerRadius = value;
        }

        public float OuterRadius
        {
            get => m_OuterRadius;
            set => m_OuterRadius = value;
        }

        public void SpawnAsteroids()
        {
            // Clean up existing
            ClearAsteroids();

            for (int i = 0; i < m_MaxAsteroids; i++)
            {
                var asteroidObj = new GameObject($"Asteroid_{i}");
                asteroidObj.transform.SetParent(transform);

                // Place randomly in the orbital belt ring
                float angle = i * (Mathf.PI * 2f / m_MaxAsteroids);
                float radius = Random.Range(m_InnerRadius, m_OuterRadius);
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                asteroidObj.transform.localPosition = pos;

                // Add logistics components so it's mineable
                var inventory = asteroidObj.AddComponent<ResourceInventory>();
                inventory.MaxCapacity = 1000;
                inventory.AddResource(ResourceType.Ore, 500); // Prefill with mineable ore

                // Add visual primitive representation for the asteroid
                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "Visual";
                visual.transform.SetParent(asteroidObj.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Random.rotation;
                
                // Random scale to make them look organic
                float size = Random.Range(5f, 15f);
                visual.transform.localScale = new Vector3(size, size * Random.Range(0.8f, 1.2f), size * Random.Range(0.8f, 1.2f));

                // Greyish-brown color for asteroids
                var renderer = visual.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Standard"));
                    material.color = new Color(0.45f, 0.4f, 0.35f);
                    renderer.sharedMaterial = material;
                }

                var collider = visual.GetComponent<Collider>();
                if (collider != null)
                {
                    DestroyImmediate(collider);
                }

                m_SpawnedAsteroids.Add(asteroidObj);
            }
            Debug.Log($"[AsteroidBelt] Spawned {m_MaxAsteroids} mineable asteroids in belt.");
        }

        public void ClearAsteroids()
        {
            foreach (var ast in m_SpawnedAsteroids)
            {
                if (ast != null)
                {
                    DestroyImmediate(ast);
                }
            }
            m_SpawnedAsteroids.Clear();
        }

        private void OnDestroy()
        {
            ClearAsteroids();
        }
    }
}
