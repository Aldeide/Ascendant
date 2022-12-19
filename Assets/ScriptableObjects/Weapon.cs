using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
[CreateAssetMenu(fileName = "Weapon", menuName = "ScriptableObjects/Weapon", order = 1)]
public class Weapon : ScriptableObject
{
    [Header("3D Models")]
    public GameObject weaponModel;
    public GameObject projectile;
    public GameObject muzzleEffect;

    [Header("Weapon Characteristics")]
    public string weaponName;
    [Tooltip("The size of the weapon's clip")]
    public int clipSize;
    [Tooltip("The weapon's fire rate per minute")]
    public int fireRate;

    public Transform GetMuzzleTransform()
    {
        return weaponModel.transform.Find("Muzzle").transform;
    }
}
