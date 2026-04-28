using Godot;
using System;

public partial class DeathScreen : Node2D {
	// Called when the node enters the scene tree for the first time.
	public void OnRestartPressed() {
		GetTree().ChangeSceneToFile("res://Levels/RunnerLevel.tscn");
	}

	public void OnQuitPressed() {
		GetTree().ChangeSceneToFile("res://TitleScreen.tscn");
	}
}
