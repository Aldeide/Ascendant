using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    public LayerMask layerMask;
    public GameObject player;

    public bool thirdPerson = true;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        if (thirdPerson)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!thirdPerson)
        {
            Plane plane = new Plane(new Vector3(0, 1, 0), player.transform.position);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float enter = 0;
            plane.Raycast(ray, out enter);
            Vector3 hit = ray.GetPoint(enter);
            hit.y += 1.5f;
            this.transform.position = hit;
            return;
        } else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000, layerMask, QueryTriggerInteraction.Ignore))
            {
                this.transform.position = hit.point;
            } else
            {
                this.transform.position = ray.GetPoint(10.0f);
            }
        }
        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(this.transform.position, 0.1f);
    }
}
