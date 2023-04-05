using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    // Cinemachine cameras.
    public GameObject defaultCamera;
    public GameObject aimCamera;
    public GameObject sprintCamera;

    // Player components.
    private PlayerStateManager stateManager;

    void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();

        defaultCamera = GameObject.Find("Camera - Third-Person");
        sprintCamera = GameObject.Find("Camera - Sprinting");
        aimCamera = GameObject.Find("Camera - Aim");
        defaultCamera.SetActive(true);
        sprintCamera.SetActive(false);
        aimCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Activate aim camera if needed. The aim camera is much closer to the player.
        if (stateManager.stanceState == PlayerStanceState.Aiming && !aimCamera.activeInHierarchy)
        {
            defaultCamera.SetActive(false);
            sprintCamera.SetActive(false);
            aimCamera.SetActive(true);
            return;
        }

        // Activate the sprint camera if needed. The sprint camera follows the player from afar.
        // Sprinting isn't possible while aiming.
        if (stateManager.movementState == PlayerMovementState.Sprinting
            && !sprintCamera.activeInHierarchy
            && stateManager.stanceState != PlayerStanceState.Aiming)
        {
            defaultCamera.SetActive(false);
            sprintCamera.SetActive(true);
            aimCamera.SetActive(false);
            return;
        }

        // Activate the default camera if needed.
        if (!defaultCamera.activeInHierarchy
            && stateManager.stanceState!= PlayerStanceState.Aiming
            && stateManager.movementState != PlayerMovementState.Sprinting)
        {
            defaultCamera.SetActive(true);
            sprintCamera.SetActive(false);
            aimCamera.SetActive(false);
        }
    }
}
