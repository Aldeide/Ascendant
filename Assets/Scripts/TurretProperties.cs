using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretProperties : MonoBehaviour
{
    public float range = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        var detectionCollider = this.GetComponent<SphereCollider>();
        if (detectionCollider == null)
        {
            return;
        }
        detectionCollider.radius = range;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
