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
    }
}
