using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponEffects : MonoBehaviour
{
    public float currentLife = 0;
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<AudioSource>().Play();
        this.GetComponent<VisualEffect>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        currentLife += Time.deltaTime;
        if(currentLife > 1.5f)
        {
            Destroy(gameObject);
        }
    }
}
