using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

namespace Ascendant
{
    [RequireComponent(typeof(PlayerStateManager))]
    public class IKManager : MonoBehaviour
    {
        public PlayerStateManager stateManager;
        public AimIK aimIK;
        public GameObject target;

        void Start()
        {
            stateManager = GetComponent<PlayerStateManager>();
            aimIK = GetComponent<AimIK>();
            target = GameObject.Find("Game/Target");
            aimIK.solver.target = target.transform;
        }


        void Update()
        {
            if ((stateManager.stanceState == PlayerStanceState.Aiming || stateManager.firingState == PlayerFiringState.Firing)
                && aimIK.enabled == false && HasSufficientDistance())
            {
                aimIK.enabled = true;
                StartCoroutine(SmoothActivate());
            }
            else if (aimIK.enabled == true && !(stateManager.stanceState == PlayerStanceState.Aiming || stateManager.firingState == PlayerFiringState.Firing))
            {
                StartCoroutine(SmoothDeactivate());
            }
            else if (aimIK.enabled == true && !HasSufficientDistance())
            {
                StartCoroutine(SmoothDeactivate());
            }
        }

        // Returns true if the target is sufficiently distant for aimIK to be used.
        // Returns false otherwise.
        bool HasSufficientDistance()
        {
            if (Vector3.Distance(target.transform.position, this.transform.position) > 1.5f)
            {
                return true;
            }
            return false;
        }

        IEnumerator SmoothActivate()
        {
            int i = 0;
            while (i < 10)
            {
                i++;
                aimIK.GetIKSolver().IKPositionWeight = i / 10.0f;
                yield return null;
            }
        }

        IEnumerator SmoothDeactivate()
        {
            int i = 0;
            while (i < 10)
            {
                i++;
                aimIK.GetIKSolver().IKPositionWeight = 1 - i / 10.0f;
                yield return null;
            }
            aimIK.enabled = false;
        }

    }

}

