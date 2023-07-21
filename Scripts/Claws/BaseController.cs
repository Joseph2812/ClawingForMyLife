using Godot;
using System;

namespace Scripts.Claws;

public partial class BaseController : RigidBody2D
{
    public float VerticalSpeed
    {
        get => _verticalSpeed;
        set
        {
            _verticalSpeed = value;
            if (_verticalSpeed == 0f) { GravityScale = -0.2f; } // To cancel out the weight of claws
            else                      { GravityScale = 1f; }
        }
    }
    private float _verticalSpeed;

    private Area2D _proximityTrigger;

    public override void _Ready()
    {
        base._Ready();

        _proximityTrigger = GetNode<Area2D>("ProximityTrigger");

        VerticalSpeed = 0f;

        _proximityTrigger.BodyEntered += OnProximityTrigger_BodyEntered;
        _proximityTrigger.BodyExited += OnProximityTrigger_BodyExited;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        state.LinearVelocity = new Vector2(state.LinearVelocity.X, VerticalSpeed);
    }

    private void OnProximityTrigger_BodyEntered(Node2D body)
    {
        if (body is PlayerController player) { player.StartPanicking(); }
    }
    private void OnProximityTrigger_BodyExited(Node2D body)
    {
        if (body is PlayerController player) { player.StopPanicking(); }
    }
}
