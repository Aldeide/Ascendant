using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FishNet;

namespace Ascendant
{
    public class FollowTarget : MonoBehaviour
    {
        public Transform target;
        // Start is called before the first frame update
        void Start()
        {
            /*
            if (GameManager.Instance.localPlayer != null)
            {
                target = GameManager.Instance.localPlayer.transform.GetComponentsInChildren<Transform>()
                        .Where(transform => transform.name == "mixamorig:Neck").First();
            }
            */
        }

        // Update is called once per frame
        void Update()
        {
            if (target == null)
            {
                target = GameManager.Instance.localPlayer.GetComponent<Player>().controlledCharacter.transform.GetComponentsInChildren<Transform>()
                    .Where(transform => transform.name == "mixamorig:Neck").First();
            }
            Vector3 targetPosition = target.transform.position;
            targetPosition.y = target.transform.position.y + 1.5f;
            this.transform.position = targetPosition;
        }
    }
}


