using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        if (target == null)
        {
            target = GameObject.Find("mixamorig:Neck");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            target = GameObject.Find("mixamorig:Neck");
        }
        Vector3 targetPosition = target.transform.position;
        targetPosition.y = target.transform.position.y + 1.5f;
        this.transform.position = targetPosition;
    }
}
