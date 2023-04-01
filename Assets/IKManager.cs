using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

[RequireComponent(typeof(PlayerStateManager))]
public class IKManager : MonoBehaviour
{
    public PlayerStateManager stateManager;
    public AimIK aimIK;
    void Start()
    {
        stateManager = GetComponent<PlayerStateManager>();
        aimIK = GetComponent<AimIK>();
    }


    void Update()
    {
        // TODO: make transition smooth.
        if (stateManager.stanceState == PlayerStanceState.Aiming || stateManager.firingState == PlayerFiringState.Firing)
        {
            aimIK.enabled = true;
        } else
        {
            aimIK.enabled = false;
        }
    }
}
