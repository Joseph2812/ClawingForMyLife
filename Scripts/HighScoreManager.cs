using Godot;
using System;

namespace Scripts;

public partial class HighScoreManager : Label
{
    private const string SaveFilePath = "user://Score.save";
    private Label _valueLabel;

    public override void _Ready()
    {
        base._Ready();

        _valueLabel = GetNode<Label>("Value");

        if (FileAccess.FileExists(SaveFilePath))
        {
            using FileAccess saveFile = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Read);
            _valueLabel.Text = saveFile.Get32().ToString();
        }     

        PointTrigger.PlayerCaughtInChute += OnPointTrigger_PlayerCaughtInChute;
    }

    private void OnPointTrigger_PlayerCaughtInChute(PlayerController _, uint finalScore)
    {
        if (FileAccess.FileExists(SaveFilePath))
        {
            using FileAccess saveFile = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.ReadWrite);

            if (saveFile.Get32() >= finalScore) { return; }
            saveFile.Seek(0ul);
            saveFile.Store32(finalScore);
        }
        else
        {
            using FileAccess saveFile = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Write);
            saveFile.Store32(finalScore);
        }
    }
}
