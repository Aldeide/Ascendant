using Ascendant.SystemsExtensions.Celestial;
using UnityEngine;

namespace Ascendant.SystemsExtensions
{
    public class SceneManager : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GetComponent<StarSystem>().InitializeSystem("SystemAlpha");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
