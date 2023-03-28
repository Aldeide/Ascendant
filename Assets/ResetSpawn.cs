using UnityEngine;
using UnityEngine.InputSystem;

public class ResetSpawn : MonoBehaviour
{
    private GameObject player;

    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        player.GetComponent<CharacterController>().enabled = false;
        Debug.Log("Respawning");
        player.transform.position = this.transform.position;
        player.GetComponent<CharacterController>().enabled = true;

    }
}
