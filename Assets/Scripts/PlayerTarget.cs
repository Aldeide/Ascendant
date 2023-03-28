using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    public LayerMask layerMask;
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        Plane plane = new Plane(new Vector3(0, 1, 0), player.transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float enter = 0;
        plane.Raycast(ray, out enter);
        Vector3 hit = ray.GetPoint(enter);
        hit.y += 1.5f;
        this.transform.position = hit;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(this.transform.position, 0.1f);
    }
}
