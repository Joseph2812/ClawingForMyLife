using Godot;
using System;
using Scripts.Claws;

namespace Scripts;

public partial class OperatorManager : Node2D
{
    private const double MaximumXImpulse = 10d;
    private const double MaximumYImpulse = 20d;

    private AnimationPlayer _animPlayer;
    private AudioStreamPlayer2D _angerSfx;
    private bool _queuedShake;

    public override void _Ready()
    {
        base._Ready();

        _animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _angerSfx = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        ClawsController.Inst.SpeedIncrease += OnClawsController_SpeedIncrease;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (!_queuedShake) { return; }
        _queuedShake = false;

        _angerSfx.Play();
        foreach (RigidBody2D rb in WarpableRigidbody2D.Instances)
        {
            rb.ApplyImpulse
            (
                ( Vector2.Right * (float)GD.RandRange(-MaximumXImpulse, MaximumXImpulse) ) + ( Vector2.Up * (float)GD.RandRange(MaximumYImpulse * 0.5f, MaximumYImpulse) )
            );
        }
    }

    private void ShakePrizes() { _queuedShake = true; }

    private void OnClawsController_SpeedIncrease() { _animPlayer.Play("Anger"); }
}
