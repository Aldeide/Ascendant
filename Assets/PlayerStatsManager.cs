using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public float currentHealth { get; set; }
    public float maxHealth { get; set; }

    public float currentShield { get; set; }

    public float maxShield { get; set; }


    // Start is called before the first frame update
    void Start()
    {
        maxHealth = 100;
        maxShield = 100;
        currentHealth = maxHealth;
        currentShield = maxShield;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Damage(float damage)
    {
        currentShield -= damage;
        if (currentShield < 0)
        {
            currentHealth += currentShield;
            currentShield = 0;
            if (currentHealth < 0)
            {
                currentHealth = 0;
            }
        }
    }
}
