using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorDirection
{
    Right,
    Left,
    Top,
    Bottom
}

public class Door : MonoBehaviour
{
    public GameObject door;
    public DoorDirection doorDirection;
    public Collider doorCollider;
    public float movementAmount;
    // Start is called before the first frame update
    void Start()
    {
        
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
            door.transform.Translate(new Vector3(-movementAmount / 10f, 0, 0));
            yield return null;
        }
    }

    IEnumerator DoorClose()
    {
        int i = 0;
        while (i < 10)
        {
            i++;
            door.transform.Translate(new Vector3(movementAmount / 10f, 0, 0));
            yield return null;
        }
    }
}
