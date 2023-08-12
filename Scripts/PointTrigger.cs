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

        _score++;
        _scoreLabel.Text = _score.ToString("D3");

        PrizeBall ball = (PrizeBall)body;
        ball.TopSelfModulate = Color.FromHsv(GD.Randf(), 1f, 1f);
        ball.WarpToGlobalPosition(new Vector2((float)GD.RandRange(_begin.GlobalPosition.X, _end.GlobalPosition.X), _begin.GlobalPosition.Y));
    }
}
