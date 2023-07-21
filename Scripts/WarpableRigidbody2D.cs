using Godot;
using System;
using System.Collections.Generic;

namespace Scripts;

public partial class WarpableRigidbody2D : RigidBody2D
{
    public static readonly List<WarpableRigidbody2D> Instances = new();

    private Vector2 _warpPosition;
    private bool _isWarping;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (!_isWarping) { return; }
        _isWarping = false;

        state.LinearVelocity = Vector2.Zero;
        state.AngularVelocity = 0f;
        state.Transform = new Transform2D(0f, _warpPosition);
    }

    public override void _EnterTree()
    {
        base._EnterTree();

        Instances.Add(this);
    }

    public override void _ExitTree()
    {
        base._ExitTree();

        Instances.Remove(this);
    }

    public void WarpToGlobalPosition(Vector2 globalPosition)
    {
        _warpPosition = globalPosition;
        _isWarping = true;
    }
}
