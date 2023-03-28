using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : MonoBehaviour
{
    private Transform mobile;
    private Transform aim;

    // Start is called before the first frame update
    void Start()
    {
        mobile = this.gameObject.transform.Find("Mobile");
        aim = this.gameObject.transform.Find("Mobile/RotationOffset/Aim");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered turret detection range");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player is in detection range");
            //mobile.LookAt(other.transform.position);

            Vector3 targetMobilePosition = new Vector3(other.transform.position.x,
                                        mobile.transform.position.y,
                                        other.transform.position.z);
            mobile.LookAt(targetMobilePosition);

            Vector3 targetAimPosition = new Vector3(aim.transform.position.x,
                                        other.transform.position.y,
                                        other.transform.position.z);

            var vect = other.transform.position - aim.position;
            vect.x = 0;
            var rot = Quaternion.LookRotation(vect);
            aim.transform.localRotation = rot;
            aim.transform.localRotation = Quaternion.Euler(aim.transform.localRotation.eulerAngles.x, -90, aim.transform.localRotation.eulerAngles.z);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited detection range");
        }
    }
}
