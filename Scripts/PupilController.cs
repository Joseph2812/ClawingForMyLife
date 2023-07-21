using Godot;
using System;
using Scripts.Claws;

namespace Scripts;

public partial class PupilController : Sprite2D
{
    private const double ShakeDuration = 0.06d;

    private Tween _tween;
    private Vector2 _offset;

    private Vector2 _leftMostCentre;
    private Vector2 _rightMostCentre;

    public override void _Ready()
    {
        base._Ready();

        _tween = CreateTween();
        _offset = Position;

        _leftMostCentre = new(_offset.X - 1f, 0f);
        _rightMostCentre = new(_offset.X + 1f, 0f);

        _tween.Kill();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_tween.IsRunning()) { return; }

        Vector2 dir = _offset.DirectionTo(ToLocal(ClawsController.Inst.BaseGlobalPosition));
        Position = new Vector2(dir.X, dir.Y * 2f) + _offset;
    }

    public void StartShaking()
    {
        _tween = CreateTween();
        _tween.SetLoops();

        _tween.TweenProperty(this, "position", _leftMostCentre, ShakeDuration);
        _tween.TweenProperty(this, "position", _rightMostCentre, ShakeDuration);
    }
    public void StopShaking() { _tween.Kill(); }
}
