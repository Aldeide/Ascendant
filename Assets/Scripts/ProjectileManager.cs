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

    private Vector3 origin = new Vector3();
    float potentialDistance = 0.0f;
    private Vector3 potentialHit = new Vector3();
    void Start()
    {
        origin = transform.position;


        // origin - potentialhit.
    }

    // Update is called once per frame
    void Update()
    {
        previousDelta = Time.deltaTime;
        currentLife += Time.deltaTime;
        if (currentLife > ttl)
        {
            Destroy(this.gameObject);
        }
        
        
        // Check for future collisions.
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);


        Physics.Raycast(ray, out hit, 1000, LayerMask.NameToLayer("Everything"), QueryTriggerInteraction.Ignore);
        if (hit.collider == null)
        {
            transform.position += speed * Time.deltaTime * transform.forward;
            return;
        }

        if (hit.collider.isTrigger)
        {
            return;
        }

        if (hit.distance * hit.distance > (speed * Time.deltaTime * transform.forward).sqrMagnitude)
        {
            potentialDistance = hit.distance;
            potentialHit = hit.point;
            transform.position += speed * Time.deltaTime * transform.forward;
            return;
        } else
        {
            impactPoint = hit.point;
            if ((hit.point - origin).magnitude > potentialDistance)
            {
                impactPoint = potentialHit;
            }
            transform.position = impactPoint;
            //Destroy(this.gameObject);
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
