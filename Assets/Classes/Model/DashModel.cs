using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Ascendant.Models
{
    public class DashModel : NetworkBehaviour
    {
        // The number of charges currently available.
        [SyncVar]
        public int charges = 3;
        // The maximum number of charges for the dash.
        [SyncVar]
        public int maxCharges = 3;
        // How quickly a charge becomes available.
        [SyncVar]
        public float chargeRate = 10f;
        // The delay before the cooldown starts after a dash charge has been used.
        [SyncVar]
        public float chargeDelay = 2f;
        [SyncVar]
        public float lastDash = 0f;
        [SyncVar]
        public float currentCooldown = 0f;
    }
}

