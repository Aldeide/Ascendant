using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

namespace Ascendant
{
    public sealed class Character : NetworkBehaviour
    {
        [SyncVar]
        public Player controllingPlayer;
    }
}


