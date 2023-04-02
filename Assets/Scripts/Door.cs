using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorDirection
{
    PosX,
    NegX,
    PosY,
    NegY
}

public class Door : MonoBehaviour
{
    public GameObject door;
    public DoorDirection doorDirection;
    public Collider doorCollider;
    public float movementAmount;
    private Vector3 movementDirection;
    // Start is called before the first frame update
    void Start()
    {
        if(doorDirection == DoorDirection.PosX)
        {
            movementDirection = new Vector3(1, 0, 0);
        } else if (doorDirection == DoorDirection.NegX)
        {
            movementDirection = new Vector3(-1, 0, 0);
        } else if (doorDirection == DoorDirection.PosY)
        {
            movementDirection = new Vector3(0, 2, 0);
        } else 
        {
            movementDirection = new Vector3(0, -1, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StartCoroutine(DoorOpen());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(DoorClose());
        }
    }

    IEnumerator DoorOpen()
    {
        int i = 0;
        while (i < 10)
        {
            i++;
            door.transform.Translate(movementAmount / 10f * movementDirection);
            yield return null;
        }
    }

    IEnumerator DoorClose()
    {
        int i = 0;
        while (i < 10)
        {
            i++;
            door.transform.Translate(-1f * movementAmount / 10f * movementDirection);
            yield return null;
        }
    }
}
