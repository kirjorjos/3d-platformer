using Godot;
using System;

public partial class TitleScreen : Node2D {

	public void OnPlayPressed() {
		GetTree().ChangeSceneToFile("res://Levels/RunnerLevel.tscn");
	}

	public void OnQuitPressed() {
		GetTree().Quit();
	}
}
