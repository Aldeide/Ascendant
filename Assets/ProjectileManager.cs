using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public float speed = 20.0f;
    public float ttl = 4.0f;
    private float currentLife = 0.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentLife += Time.deltaTime;
        if (currentLife > ttl)
        {
            Destroy(this.gameObject);
        }
        transform.position += speed * Time.deltaTime * transform.forward;
        
    }
}
