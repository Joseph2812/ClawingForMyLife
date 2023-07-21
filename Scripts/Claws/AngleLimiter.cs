using Godot;
using System;

namespace Scripts.Claws;

public partial class AngleLimiter : RigidBody2D
{
    [Export(PropertyHint.Range, "-360,360,15,radians")]
    private float _rotationLowerLimit;
    [Export(PropertyHint.Range, "-360,360,15,radians")]
    private float _rotationUpperLimit;

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);

        if (Rotation < _rotationLowerLimit || Rotation > _rotationUpperLimit)
        {
            state.AngularVelocity = 0f;
            state.Transform = new Transform2D(Mathf.Clamp(Rotation, _rotationLowerLimit, _rotationUpperLimit), state.Transform.Origin);
        }
    }
}
