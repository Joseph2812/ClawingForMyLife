using Godot;
using System;

namespace Scripts;

public partial class PrizeBall : WarpableRigidbody2D
{
    private const float InverseMaxVolumeSpeed = 1f / 500f;
    private const float MinimumVolumeDb = -80f;

    public Color TopSelfModulate
    {
        set { _top.SelfModulate = value; }
    }

    private CanvasItem _top;
    private AudioStreamPlayer2D _hitSfx;

    private Vector2 _lastContactPoint;
    private float _lastNormalSpeed;

    public override void _Ready()
    {
        base._Ready();

        _top = GetNode<CanvasItem>("Top");
        _hitSfx = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        BodyEntered += OnBodyEntered;
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (GetContactCount() == 0) { return; }

        _lastContactPoint = state.GetContactLocalPosition(0);
        _lastNormalSpeed = state.GetContactLocalVelocityAtPosition(0).Dot(-state.GetContactLocalNormal(0));
    }

    private void OnBodyEntered(Node body)
    {
        _hitSfx.GlobalPosition = _lastContactPoint;
        _hitSfx.VolumeDb = (_lastNormalSpeed > 0f) ?
            Mathf.Clamp(Mathf.LinearToDb(_lastNormalSpeed * InverseMaxVolumeSpeed), MinimumVolumeDb, 0f) :
            MinimumVolumeDb;

        _hitSfx.Play();
    }
}
