using Godot;
using System;

public partial class Coin : Area3D {
	private static double ROTATION_SPEED = 2.0;
	public override void _Ready() {

	}

	public override void _Process(double delta) {
		RotateY((float)(ROTATION_SPEED * delta));
	}

	public void OnBodyEntered() {
		QueueFree();
	}
}
