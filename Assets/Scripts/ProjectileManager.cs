using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
public class ProjectileManager : MonoBehaviour
{
    public float speed = 40.0f;
    public float ttl = 4.0f;
    private float currentLife = 0.0f;
    private float previousDelta = 0.0f;
    private Vector3 impactPoint;
    public LayerMask layerMask;
    private Vector3 origin = new Vector3();
    float potentialDistance = 0.0f;
    private Vector3 potentialHit = new Vector3();

    public VisualEffect impactEffect;
    private bool collided = false;
    void Start()
    {
        origin = transform.position;
        impactEffect = GetComponentInChildren<VisualEffect>();
        impactEffect.Stop();
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
        if (collided)
        {
            return;
        }
        
        // Check for future collisions.
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);


        Physics.Raycast(ray, out hit, 1000, layerMask, QueryTriggerInteraction.Ignore);
        if (hit.collider == null)
        {
            transform.position += speed * Time.deltaTime * transform.forward;
            return;
        }

        if (hit.collider.isTrigger)
        {
            return;
        }
        impactPoint = hit.point;
        if (impactEffect != null && !collided)
        {
            this.transform.position = impactPoint;
            impactEffect.transform.position = impactPoint;
            impactEffect.transform.rotation = Quaternion.FromToRotation(impactEffect.transform.up, hit.normal) * impactEffect.transform.rotation;

            impactEffect.Play();
        }
        collided = true;

        if (hit.collider.GetComponent<PlayerStatsManager>() != null)
        {
            hit.collider.GetComponent<PlayerStatsManager>().Damage(20.0f);
        }

        /*

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
                if (impactEffect != null && !collided)
                {
                    impactEffect.transform.position = impactPoint;
                    impactEffect.Play();
                }
                collided = true;

            }
            transform.position = impactPoint;
            //Destroy(this.gameObject);
        }
        */
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
