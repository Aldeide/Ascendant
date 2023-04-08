using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PlayerInputComponent : IComponentData
{
    public float2 movementInput;
    public float2 lookInput;
}
