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
    private bool playedImpactSound = false;
    public AudioClip defaultImpactAudio;

    void Start()
    {
        origin = transform.position;
        impactEffect = GetComponentInChildren<VisualEffect>();
        impactEffect.Stop();
        this.GetComponent<AudioSource>().Play();
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        collided = Physics.Raycast(ray, out hit, 1000, layerMask, QueryTriggerInteraction.Ignore);
        impactPoint = hit.point;
        if (impactEffect != null)
        {
            impactEffect.transform.position = impactPoint;
            //impactEffect.transform.rotation = Quaternion.FromToRotation(impactEffect.transform.up, hit.normal) * impactEffect.transform.rotation;
            impactEffect.transform.forward = hit.normal;
            impactEffect.Play();
            
        }

        

        if (hit.collider.GetComponent<Ascendant.Controllers.PlayerStatsController>() != null)
        {
            Debug.Log("hit");
            hit.collider.GetComponent<Ascendant.Controllers.PlayerStatsController>().Damage(2.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentLife += Time.deltaTime;
        if(currentLife > ttl)
        {
            Destroy(this.gameObject);
        }

        if (collided)
        {
            this.transform.position = impactPoint;
            if (!playedImpactSound)
            {
                this.GetComponent<AudioSource>().PlayOneShot(defaultImpactAudio);
                playedImpactSound = true;
            }
        } else
        {
            this.transform.position = this.transform.position + this.transform.forward * 1000;
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
