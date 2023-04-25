using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WeaponEffects : MonoBehaviour
{
    public float currentLife = 0;
    private bool played = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!played)
        {
            
            played = true;
        }
        currentLife += Time.deltaTime;
        if(currentLife > 1.5f)
        {
            Destroy(gameObject);
        }
    }

    public void PlayEffects()
    {
        this.GetComponent<AudioSource>().Play();
        this.GetComponent<VisualEffect>().Play();
    }
}
