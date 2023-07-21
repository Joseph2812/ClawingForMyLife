using Godot;
using System;
using System.Collections.Generic;

namespace Scripts;

public partial class PointTrigger : Area2D
{
    public static event Action<PlayerController, uint> PlayerCaughtInChute; // (PlayerInstance, FinalScore)

    private uint _score;
    private Label _scoreLabel;
    private Node2D _begin;
    private Node2D _end;
    private AudioStreamPlayer2D _chuteSfx;

    private readonly List<WarpableRigidbody2D> _bodiesToMove = new();

    public PointTrigger() { PlayerCaughtInChute = null; }

    public override void _Ready()
    {
        base._Ready();

        _scoreLabel = GetNode<Label>("Score");
        _begin = GetNode<Node2D>("Begin");
        _end = GetNode<Node2D>("End");
        _chuteSfx = GetNode<AudioStreamPlayer2D>("AudioStreamPlayer2D");

        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (_bodiesToMove.Count == 0) { return; }
        
        foreach (WarpableRigidbody2D body in _bodiesToMove)
        {
            body.WarpToGlobalPosition(new Vector2((float)GD.RandRange(_begin.GlobalPosition.X, _end.GlobalPosition.X), _begin.GlobalPosition.Y));
        }
        _bodiesToMove.Clear();
    }

    private void OnBodyEntered(Node body)
    {
        _chuteSfx.Play();
        if (body is PlayerController player)
        {
            _scoreLabel.Text = "DED";
            GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
            PlayerCaughtInChute?.Invoke(player, _score);

            return;
        }

        _score += 1;
        _scoreLabel.Text = _score.ToString("D3");

        PrizeBall wRb = (PrizeBall)body;
        _bodiesToMove.Add(wRb);
        wRb.TopSelfModulate = Color.FromHsv(GD.Randf(), 1f, 1f);
    }
}
