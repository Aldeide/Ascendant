using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using Ascendant.NetworkData;

namespace Ascendant.Controllers
{
    // Controller for the dash movement. Handles the updating of the available charges, their cooldowns
    // and the actual dash movement logic. This uses client prediction.
    public sealed class DashController : NetworkBehaviour
    {
        public Models.DashModel dashModel;
        public bool isDashing = false;
        private CharacterController characterController;
        private PlayerMovementController playerMovementController;
        private bool dashRequested = false;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            base.TimeManager.OnTick += TimeManagerOnTick;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            if (base.TimeManager != null)
            {
                base.TimeManager.OnTick -= TimeManagerOnTick;
            }
        }
        private void TimeManagerOnTick()
        {
            if (!dashRequested)
            {
                return;
            }
            if (base.IsOwner)
            {
                Reconcile(default, false);
                BuildActions(out DashData dashData);
                Dash(dashData, false);
            }
            if (IsServer)
            {
                Dash(default, true);
                DashReconcileData rd = new DashReconcileData()
                {
                    position = transform.position,
                    rotation = transform.rotation,
                    charges = dashModel.charges,
                    lastDash = dashModel.lastDash
                };
                Reconcile(rd, true);
            }
            dashRequested = false;
        }
        private void BuildActions(out DashData dashData)
        {
            dashData = default;
            dashData.isDashing = this.isDashing;
            dashData.charges = this.dashModel.charges;
            dashData.lastDash = dashModel.lastDash;
        }

        [Replicate]
        private void Dash(DashData dashData, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {
            if (dashData.isDashing) return;
            if (dashData.charges == 0) return;

            if (asServer)
            {
                dashModel.charges -= 1;
                dashModel.lastDash = 0;
            }
            
            Vector3 direction = playerMovementController.direction;
            direction.Normalize();
            characterController.Move(direction * 3.0f);
        }

        [Reconcile]
        private void Reconcile(DashReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
        {
            transform.position = data.position;
            transform.rotation = data.rotation;
            dashModel.charges = data.charges;
            dashModel.lastDash = data.lastDash;
        }

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            playerMovementController = GetComponent<PlayerMovementController>();
        }

        void Update()
        {
            if (dashModel.charges < dashModel.maxCharges)
            {
                dashModel.lastDash += Time.deltaTime;
                if (dashModel.lastDash > dashModel.chargeDelay)
                {
                    dashModel.currentCooldown += Time.deltaTime;
                    if (dashModel.currentCooldown > dashModel.chargeRate)
                    {
                        dashModel.charges += 1;
                        dashModel.currentCooldown = 0;
                    }
                }
                return;
            }
        }

        public void TryDash()
        {
            dashRequested = true;
            
        }

        public bool CanDash()
        {
            if (isDashing) return false;
            if (dashModel.charges == 0) return false;
            return true;
        }

        private IEnumerator Dash()
        {
            isDashing = true;
            Vector3 direction = playerMovementController.direction;
            direction.Normalize();
            characterController.Move(direction * 3.0f);
            yield return new WaitForSeconds(1.0f);
            isDashing = false;
        }

    }
}


