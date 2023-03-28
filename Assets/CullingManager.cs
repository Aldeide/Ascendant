using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullingManager : MonoBehaviour
{
    private GameObject player;

    private List<Collider> colliders = new List<Collider>();
    private List<Collider> colliders2 = new List<Collider>();
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        RaycastHit hit;

        Vector3 pos = player.transform.position;
        pos.y += 1.0f;

        if (Physics.SphereCast(pos, 0.5f, Camera.main.transform.position - pos, out hit, 40))
        {
            if (hit.collider.GetComponent<CullZone>() != null)
            {
                foreach(var renderer in hit.collider.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }
                colliders2.Add(hit.collider);
            }


            if (hit.collider.GetComponent<CullObject>() == null)
            {
                foreach (var collider in colliders)
                {
                    var test2 = collider.GetComponent<CullObject>();
                    if (test2 == null)
                    {
                        return;
                    }
                    test2.UnCull();
                }
            }


            Debug.DrawLine(Camera.main.transform.position, hit.point);
            var test = hit.collider.GetComponent<CullObject>();
            if (test == null)
            {
                return;
            }
            colliders.Add(hit.collider);
            test.Cull();
        } else
        {
            foreach (var collider in colliders2)
            {
                foreach(var renderer in collider.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = true;
                }
            }
            foreach (var collider in colliders)
            {
                var test2 = collider.GetComponent<CullObject>();
                if (test2 == null)
                {
                    return;
                }
                test2.UnCull();
            }
            colliders = new List<Collider>();
            colliders2 = new List<Collider>();
        }
    }
}
