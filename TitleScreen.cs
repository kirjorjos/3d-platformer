using Godot;
using System;

public partial class TitleScreen : Node2D {
	TextureButton playButton;
	TextureButton quitButton;

	public override void _Ready() {
		 playButton = GetNode<TextureButton>("Play");
		 quitButton = GetNode<TextureButton>("Quit");

		 playButton.GrabFocus();
	}

	public void OnPlayPressed() {
		GetTree().ChangeSceneToFile("res://Levels/RunnerLevel.tscn");
	}

	public void OnQuitPressed() {
		GetTree().Quit();
	}
}
