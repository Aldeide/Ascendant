using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ascendant
{
    public class PlayerTarget : MonoBehaviour
    {
        public LayerMask layerMask;
        public GameObject player;

        private Vector3 newTarget = new Vector3();

        void Start()
        {
            if (GameManager.Instance.localPlayer != null)
            {
                player = GameManager.Instance.localPlayer;
            }
            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            if (player == null)
            {
                player = GameManager.Instance.localPlayer;
            }
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000, layerMask, QueryTriggerInteraction.Ignore))
            {
                newTarget = hit.point;
            }
            else
            {
                newTarget = ray.GetPoint(10.0f);
            }
            transform.position = Vector3.Lerp(this.transform.position, newTarget, Time.deltaTime * 8.0f);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(this.transform.position, 0.1f);
        }
    }
}


