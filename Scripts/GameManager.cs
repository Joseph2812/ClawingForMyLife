using Godot;
using System;

namespace Scripts;

public partial class GameManager : Node
{
    public static event Action GameStart;

    private AudioStreamPlayer _musicPlayer;

    public GameManager() { GameStart = null; }

    public override void _Ready()
    {
        base._Ready();

        _musicPlayer = GetNode<AudioStreamPlayer>("/root/Main/MusicPlayer");

        _musicPlayer.CreateTween().TweenProperty(_musicPlayer, "volume_db", 0f, 3f);

        PointTrigger.PlayerCaughtInChute += OnPointTrigger_PlayerCaughtInChute;
    }

    private void ReloadScene() { GetTree().ReloadCurrentScene(); }

    private void OnPointTrigger_PlayerCaughtInChute(PlayerController player, uint _)
    {
        player.Death();
        GetNode<AnimationPlayer>("/root/Main/UI/GameOverScreen/AnimationPlayer").Play("game_over");
        _musicPlayer.CreateTween().TweenProperty(_musicPlayer, "pitch_scale", 0.75f, 1f);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if (@event.IsActionPressed("charge"))
        {
            SetProcessUnhandledInput(false);
            GameStart?.Invoke();
        }
    }
}
