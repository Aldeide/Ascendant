using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Projectile", menuName = "ScriptableObjects/Projectile", order = 1)]
public class Projectile : ScriptableObject
{
    [Header("3D Models")]
    public GameObject projectileModel;
    public GameObject projectileTrail;
    public GameObject projectileImpact;

    public float speed = 1.0f;
    public string projectileName = "";
    public float dropCoefficient = 0.0f;

}
