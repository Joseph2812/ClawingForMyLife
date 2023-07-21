using Godot;
using Scripts.Claws;
using System;

namespace Scripts;

public partial class PlayerController : PrizeBall
{
    private const float MaximumImpulse = 30f;
    private const float TimeToCharge = 1f;
    private const float InverseTimeToCharge = 1f / TimeToCharge;
    private const float SqrThresholdSpeedForNoMove = 5f * 5f;

    public static PlayerController Inst { get; private set; }

    private PupilController[] _pupilControllers;
    private GpuParticles2D _sweat;

    private Node2D _arrowPivot;
    private Godot.Range _chargeBar;

    private bool _canMove
    {
        get => p_canMove;
        set
        {
            p_canMove = value;
            _arrowPivot.Visible = value;

            if (value) { return; }

            // Reset Values //
            _chargeBar.Value = 0f;
            _timeCharging = 0f;
        }
    }
    private bool p_canMove;

    private bool _pressedCharge;
    private float _timeCharging;
    private float _impulseToApply;

    public PlayerController() { Inst = this; }

    public override void _Ready()
    {
        base._Ready();

        _pupilControllers = new PupilController[] { GetNode<PupilController>("Eyes/Pupil_L"), GetNode<PupilController>("Eyes/Pupil_R") };
        _sweat = GetNode<GpuParticles2D>("Sweat");

        _arrowPivot = GetNode<Node2D>("Pivot");
        _chargeBar = _arrowPivot.GetNode<Godot.Range>("ChargeBar");

        _chargeBar.MaxValue = TimeToCharge;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        _arrowPivot.GlobalRotation = _arrowPivot.GlobalPosition.AngleToPoint(GetGlobalMousePosition());

        if (!_canMove || !_pressedCharge) { return; }

        _timeCharging = Mathf.Clamp(_timeCharging + (float)delta, 0f, TimeToCharge);
        _chargeBar.Value = _timeCharging;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        _canMove = LinearVelocity.LengthSquared() < SqrThresholdSpeedForNoMove;

        if (_impulseToApply == 0f) { return; }

        ApplyImpulse(_impulseToApply * _arrowPivot.GlobalPosition.DirectionTo(GetGlobalMousePosition()));
        _impulseToApply = 0f;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        const string ChargeName = "charge";

        if (@event.IsActionPressed(ChargeName)) { _pressedCharge = true; }
        else if (@event.IsActionReleased(ChargeName))
        {
            _pressedCharge = false;

            _impulseToApply = (_timeCharging * InverseTimeToCharge) * MaximumImpulse;
            _canMove = false;
        }
    }

    public void StartPanicking()
    {
        foreach (PupilController pupil in _pupilControllers) { pupil.StartShaking(); }
        _sweat.Emitting = true;
    }
    public void StopPanicking()
    {
        foreach (PupilController pupil in _pupilControllers) { pupil.StopShaking(); }
        _sweat.Emitting = false;
    }

    public void Death()
    {
        SetProcess(false);
        SetPhysicsProcess(false);
        SetProcessUnhandledInput(false);

        Inst = null;
        QueueFree();
    }
}
