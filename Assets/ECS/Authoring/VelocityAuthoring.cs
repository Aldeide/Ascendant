using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

class VelocityAuthoring : MonoBehaviour
{
    public Vector3 velocity;
}

class VelocityBaker : Baker<VelocityAuthoring>
{
    public override void Bake(VelocityAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new VelocityComponent
        {
            velocity = new float3(authoring.velocity.x, authoring.velocity.y, authoring.velocity.z)
        });
    }
}

