using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant
{
    public class DamageInCollider : MonoBehaviour
    {
        public float damage = 10.0f;
        public float tickDelay = 1.0f;
        public float nextTick = 0;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time > nextTick)
            {
                if (other.CompareTag("Player"))
                {
                    other.GetComponent<Controllers.PlayerStatsController>().Damage(damage);
                    nextTick = Time.time + tickDelay;
                }
            }
        }

    }
}

