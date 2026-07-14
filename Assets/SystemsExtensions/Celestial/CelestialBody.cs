using UnityEngine;

namespace Ascendant.SystemsExtensions.Celestial
{
    public enum CelestialType
    {
        Star,
        TerrestrialPlanet,
        GaseousPlanet,
        Moon,
        AsteroidBelt
    }

    public class CelestialBody : MonoBehaviour
    {
        [SerializeField] private string m_BodyName;
        [SerializeField] private CelestialType m_Type;
        [SerializeField] private float m_Radius = 100f;
        [SerializeField] private CelestialBody m_ParentBody;

        public string BodyName
        {
            get => m_BodyName;
            set => m_BodyName = value;
        }

        public CelestialType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public float Radius
        {
            get => m_Radius;
            set => m_Radius = value;
        }

        public CelestialBody ParentBody
        {
            get => m_ParentBody;
            set => m_ParentBody = value;
        }

        public void CreateVisualRepresentation(Color color)
        {
            // Clean up existing visual child if any
            var existing = transform.Find("Visual");
            if (existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }

            // Create a primitive sphere for the visual
            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = "Visual";
            visual.transform.SetParent(transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            
            // Set scale based on Radius
            visual.transform.localScale = Vector3.one * (m_Radius * 2f);

            // Give it a colored material using standard shaders
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material starMat = null;
#if UNITY_EDITOR
                if (m_Type == CelestialType.Star)
                {
                    starMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Star.mat");
                }
#endif
                if (starMat != null)
                {
                    renderer.sharedMaterial = starMat;
                }
                else
                {
                    var material = new Material(Shader.Find("Standard"));
                    material.color = color;
                    if (m_Type == CelestialType.Star)
                    {
                        material.EnableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", color * 2f);
                    }
                    renderer.sharedMaterial = material;
                }
            }

            // Remove the collider from the visual child to keep root clean
            var collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyImmediate(collider);
            }
        }
    }
}
