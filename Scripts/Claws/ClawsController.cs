using Godot;
using System;

namespace Scripts.Claws;

public partial class ClawsController : Node2D
{
    private const float SpeedInitial = 50f;
    private const float SpeedCap = 250f;
    private const float SpeedStep = 25f;

    private const float Torque = 20000f;
    private const int DropsPerSpeedIncrease = 5;

    private const float ChuteLocalX = -33f;
    private const float MaximumGrabTime = 5f; // Failsafe if claw gets stuck before fully lowering

    private enum State
    {
        Grabbing,
        Returning,
        Dropping
    }

    public static ClawsController Inst { get; private set; }

    private static readonly Vector2 _localLowerBound = new(1f, 13f);
    private static readonly Vector2 _localUpperBound = new(33f, 88f);

    public event Action SpeedIncrease;

    public Vector2 BaseGlobalPosition => _baseController.GlobalPosition;

    private Tween _tween;
    private State _state = State.Dropping;

    private float _speed
    {
        get => p_speed;
        set
        {
            p_speed = value;
            _inverseSpeed = 1f / value;
        }
    }
    private float p_speed = SpeedInitial;
    private float _inverseSpeed = 1f / SpeedInitial;

    private float _timeElapsedInGrabbing;
    private int _dropCount;

    private Node2D _root;
    private GrooveJoint2D _joint;
    private BaseController _baseController;

	private RigidBody2D _clawL;
	private RigidBody2D _clawR;
    private AudioStreamPlayer2D _clawSfx;

    private readonly Callable _positionAndGrabCall;
    private readonly Callable _openClawsCall;
    private readonly Callable _closeClawsCall;
    private readonly Callable _increaseSpeedCall;
    private readonly Callable _baseUpCall;
    private readonly Callable _baseDownCall;  

    public ClawsController()
    { 
        Inst = this;

        _positionAndGrabCall = Callable.From(PositionAndGrab);
        _openClawsCall       = Callable.From(OpenClaws);
        _closeClawsCall      = Callable.From(CloseClaws);
        _increaseSpeedCall   = Callable.From(IncreaseSpeed);
        _baseUpCall          = Callable.From(() => _baseController.VerticalSpeed = -_speed);
        _baseDownCall        = Callable.From(() => _baseController.VerticalSpeed = _speed);
    }

    public override void _Ready()
    {
        base._Ready();

        _tween = CreateTween();

        _root = GetNode<Node2D>("Root");
        _joint = _root.GetNode<GrooveJoint2D>("RootToBase");

        _baseController = GetNode<BaseController>("Base");
        _clawL = GetNode<RigidBody2D>("Claw_L");
        _clawR = GetNode<RigidBody2D>("Claw_R");
        _clawSfx = _baseController.GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        _tween.Kill();

        GameManager.GameStart += OnGameManager_GameStart;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        QueueRedraw();

        // State Changes //
        switch (_state)
        {
            case State.Grabbing:
                _timeElapsedInGrabbing += (float)delta;
                if (_baseController.Position.Y >= _localUpperBound.Y || _timeElapsedInGrabbing >= MaximumGrabTime)
                {
                    _timeElapsedInGrabbing = 0f;
                    ReturnToTop();
                }
                break;

            case State.Returning:
                if (_baseController.Position.Y <= _localLowerBound.Y) { DropInChute(); }
                break;
        }
    }

    public override void _Draw()
    {
        base._Draw();

        DrawLine(_root.Position, _baseController.Position, Colors.Black, 1f);
    }

    private void PositionAndGrab()
    {
        if (PlayerController.Inst == null) { return; }

        _state = State.Grabbing;

        _tween.Kill();
        _tween = CreateTween();
        _tween.SetProcessMode(Tween.TweenProcessMode.Physics);

        float targetLocalX = Mathf.Clamp
        (
            ToLocal(PlayerController.Inst.GlobalPosition).X,
            _localLowerBound.X,
            _localUpperBound.X
         );
        _tween.TweenProperty(_root, "position:x", targetLocalX, GetDuration(_root.Position.X, targetLocalX));
        _tween.TweenCallback(_openClawsCall);
        _tween.TweenInterval(1d);
        _tween.TweenCallback(_baseDownCall);
        _tween.Play();
    }

    private void ReturnToTop()
    {
        _state = State.Returning;

        CloseClaws();
        _baseController.VerticalSpeed = 0f;

        _tween.Kill();
        _tween = CreateTween();
        _tween.SetProcessMode(Tween.TweenProcessMode.Physics);

        _tween.TweenInterval(1d);
        _tween.TweenCallback(_baseUpCall);
        _tween.Play();
    }

    private void DropInChute()
    {
        _state = State.Dropping;

        _baseController.VerticalSpeed = 0f;

        _tween.Kill();
        _tween = CreateTween();
        _tween.SetProcessMode(Tween.TweenProcessMode.Physics);

        _tween.TweenProperty(_root, "position:x", ChuteLocalX, GetDuration(_root.Position.X, ChuteLocalX));
        _tween.TweenCallback(_openClawsCall);
        _tween.TweenInterval(1d);
        _tween.TweenCallback(_closeClawsCall);
        _tween.TweenCallback(_increaseSpeedCall);
        _tween.TweenInterval(1d);
        _tween.TweenCallback(_positionAndGrabCall);
        _tween.Play();
    }

    private void OpenClaws()
    {
        _clawL.ConstantTorque = Torque;
        _clawR.ConstantTorque = -Torque;

        _clawSfx.Play();
    }
    private void CloseClaws()
    {
        _clawL.ConstantTorque = -Torque;
        _clawR.ConstantTorque = Torque;

        _clawSfx.Play();
    }

    private void IncreaseSpeed()
    {
        if (++_dropCount == DropsPerSpeedIncrease)
        {
            _dropCount = 0;
            _speed = Mathf.Clamp(_speed + SpeedStep, SpeedInitial, SpeedCap);

            SpeedIncrease?.Invoke();
        }
    }

    private float GetDuration(float fromX, float toX) => Mathf.Abs(toX - fromX) * _inverseSpeed;

    private void OnGameManager_GameStart() { PositionAndGrab(); }
}
