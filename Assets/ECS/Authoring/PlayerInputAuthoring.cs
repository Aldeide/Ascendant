using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

class PlayerInputAuthoring : MonoBehaviour
{
    public Vector3 velocity;
}

class PlayerInputBaker : Baker<PlayerInputAuthoring>
{
    public override void Bake(PlayerInputAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new PlayerInputComponent
        {
            movementInput = new float2(0, 0),
            lookInput = new float2(0, 0)
        });
    }
}

