using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullLevel : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject player;
    public bool isEnabled = true;
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player.transform.position.y + 5.0f < transform.position.y && isEnabled)
        {
            foreach(var item in this.GetComponentsInChildren<Renderer>())
            {
                item.enabled = false;
            }
            isEnabled = false;
        } else if (player.transform.position.y + 5.0f >= transform.position.y)
        {
            foreach (var item in this.GetComponentsInChildren<Renderer>())
            {
                item.enabled = true;
            }
            isEnabled = true;
        }
    }
}
