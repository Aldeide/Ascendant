using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnJoinClick()
    {
        var connectionManager = GameObject.Find("ConnectionManager");
        connectionManager.GetComponent<Ascendant.ConnectionManager>().InitiateConnection();
        Debug.Log("Clicked and initiating connection.");
    }
}
