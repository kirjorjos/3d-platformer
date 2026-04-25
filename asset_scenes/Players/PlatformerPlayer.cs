using Godot;
using System;

public partial class PlatformerPlayer : Player {

	public override void _Ready() {
		neck = GetNode<Node3D>("Neck");
		camera = GetNode<Camera3D>("Neck/Camera3D");
	}

	protected override Vector3 HandleActiveMovement(Vector3 velocity, double delta) {
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForwards", "MoveBackwards");
		Vector3 inputDirection = (neck.GlobalTransform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		float inputStrength = inputDir.Length();
		inputDirection *= Mathf.Min(inputStrength, 1.0f);

		if (inputDirection != Vector3.Zero) {
			velocity.X = inputDirection.X * Speed;
			velocity.Z = inputDirection.Z * Speed;
		}
		else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		return velocity;
	}
}
