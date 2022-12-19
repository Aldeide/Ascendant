using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public float speed = 40.0f;
    public float ttl = 4.0f;
    private float currentLife = 0.0f;
    private float previousDelta = 0.0f;
    private Vector3 impactPoint;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        previousDelta = Time.deltaTime;
        currentLife += Time.deltaTime;
        if (currentLife > ttl)
        {
            Destroy(this.gameObject);
        }
        transform.position += speed * Time.deltaTime * transform.forward;
        
        // Check for future collisions.
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit);
        if (hit.collider == null)
        {
            return;
        }
        if (hit.distance * hit.distance > (speed * Time.deltaTime * transform.forward).sqrMagnitude)
        {
            return;
        } else
        {
            impactPoint = hit.point;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (impactPoint != null)
        {
            Gizmos.DrawSphere(impactPoint, 0.05f);
        }
        
    }
}
