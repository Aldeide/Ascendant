using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant
{
    public enum HitLocation
    {
        Head =0,
        Neck = 1,
        Spine1 = 2,
        Spine2 = 3,
        Spine3 = 4,
        Hips = 5,
        RightUpperLeg = 6,
        RightLowerLeg = 7,
        RightFoot = 8,
        RightArm = 9,
        RightForearm = 10,
        RightHand = 11,
        LeftUpperLeg = 12,
        LeftLowerLeg = 13,
        LeftFoot = 14,
        LeftArm = 15,
        LeftForearm = 16,
        LeftHand = 17,
    }
    public class HitboxElement : MonoBehaviour
    {
        public HitLocation hitLocation;
        public Controllers.PlayerStatsController controller;
        // Start is called before the first frame update
        void Start()
        {
            controller = GetComponentInParent<Controllers.PlayerStatsController>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Damage(float damage)
        {
            controller.Damage(damage);
        }
    
}
}


