using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

namespace Ascendant.Controllers
{
    [RequireComponent(typeof(PlayerStateController))]
    public class IKController : MonoBehaviour
    {
        public PlayerStateController stateController;
        public AimIK aimIK;
        public Transform target;

        void Awake()
        {
            stateController = GetComponent<PlayerStateController>();
            aimIK = GetComponent<AimIK>();
            
            
        }

        private void Start()
        {
            aimIK.solver.target = target;
        }

        void Update()
        {
            if ((stateController.IsAiming() || stateController.IsFiring())
                && aimIK.enabled == false && HasSufficientDistance())
            {
                aimIK.enabled = true;
                StartCoroutine(SmoothActivate());
            }
            else if (aimIK.enabled == true && !(stateController.IsAiming() || stateController.IsFiring()))
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
            if (Vector3.Distance(stateController.aimTarget.transform.position, this.transform.position) > 1.5f)
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

