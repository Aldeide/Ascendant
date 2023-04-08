using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.InputSystem;
using Unity.Mathematics;
public partial class PlayerInputSystem : SystemBase
{
    private AscendantInput input;
    private AscendantInput.PlayerActions actions;

    protected override void OnCreate()
    {
        input = new AscendantInput();
        input.Enable();
        actions = input.Player;
    }

    protected override void OnDestroy() { }

    protected override void OnUpdate()
    {
        InputSystem.Update();
        Vector2 movementInput = actions.Move.ReadValue<Vector2>();
        float2 movement = new float2(movementInput.x, movementInput.y);
        foreach(var playerInputComponent in SystemAPI.Query<RefRW<PlayerInputComponent>>())
        {
            playerInputComponent.ValueRW.movementInput = movementInput;
        }
    }

}
