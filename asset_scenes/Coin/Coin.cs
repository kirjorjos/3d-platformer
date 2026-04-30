using Godot;
using System;

public partial class Coin : Area3D {
	private static double ROTATION_SPEED = 2.0;
	private static int SCORE_VALUE = 10;
	public override void _Ready() {

	}

	public override void _Process(double delta) {
		RotateY((float)(ROTATION_SPEED * delta));
	}

	public void OnBodyEntered(Node3D body) {
		if (body is CharacterBody3D playerNode) {
			RunnerPlayer player = (RunnerPlayer)playerNode;
			player.UpdateScore(SCORE_VALUE);
			GD.Print("Score updated");
			QueueFree();
		}
	}
}
