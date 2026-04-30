using Godot;
using System;

public partial class DeathScreen : Node2D {
	TextureButton restartButton;
	TextureButton quitButton;

	public override void _Ready() {
		 restartButton = GetNode<TextureButton>("Restart");
		 quitButton = GetNode<TextureButton>("QuitToTitle");

		 restartButton.GrabFocus();
	}
	public void OnRestartPressed() {
		GetTree().ChangeSceneToFile("res://Levels/RunnerLevel.tscn");
	}

	public void OnQuitPressed() {
		GetTree().ChangeSceneToFile("res://TitleScreen.tscn");
	}
}
