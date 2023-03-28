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

    [Header("Positioning")]
    public Vector3 aimingDownSightsPosition;
    public Vector3 aimingDownSightsRotation;
    public Vector3 hipFirePosition;
    public Vector3 hipFireRotation;
    public float adsDelay;

    public Vector3 crouchPosition;
    public Vector3 crouchRotation;

    public Vector3 sprintPosition;
    public Vector3 sprintRotation;

    [Header("Audio")]
    public AudioClip fireAudio;

    [Header("AnimationInterpolation")]
    public AnimationCurve aimDownSightsAnimationCurve;

    [Header("Sway")]
    public float translationSwayFactor;
    public float maxTranslationSway;
    public float translationSwaySmoothingFactor;
    public float rotationSwayFactor;
    public float maxRotationSway;
    public float rotationSwaySmoothingFactor;
    public Transform GetMuzzleTransform()
    {
        return weaponModel.transform.Find("Muzzle").transform;
    }
}
