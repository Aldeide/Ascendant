using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    [Header("3D Models")]
    public GameObject weaponModel;
    public GameObject projectile;

    [Header("Weapon Characteristics")]
    [Tooltip("The size of the weapon's clip")]
    public int clipSize;
    [Tooltip("The weaponb's fire rate per minute")]
    public int fireRate;
}
