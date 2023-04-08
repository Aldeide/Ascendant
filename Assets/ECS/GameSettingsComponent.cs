using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct GameSettingsComponent : IComponentData
{
    public float name;
}

public class GameSettingsMonoBehaviour : MonoBehaviour
{
    public GameObject go;
}

public class GameSettingsBaker : Baker<GameSettingsMonoBehaviour>
{
    public override void Bake(GameSettingsMonoBehaviour authoring)
    {
        AddComponent(new GameSettingsComponent { name = 1});
    }
}
