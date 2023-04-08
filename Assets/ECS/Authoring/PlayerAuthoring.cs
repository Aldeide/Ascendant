using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PlayerAuthoring : MonoBehaviour
{

}

public class PlayerBaker : Baker<PlayerAuthoring>
{
    public override void Bake(PlayerAuthoring authoring)
    {
    }
}
