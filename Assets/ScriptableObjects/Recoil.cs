using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
[CreateAssetMenu(fileName = "Recoil", menuName = "ScriptableObjects/Recoil", order = 1)]
public class Recoil : ScriptableObject
{
    [Header("Hipfire Recoil")]
    public float recoilX;
    public float recoilY;
    public float recoilZ;

    [Header("ADS Recoil")]
    public float aimRecoilX;
    public float aimRecoilY;
    public float aimRecoilZ;

}
