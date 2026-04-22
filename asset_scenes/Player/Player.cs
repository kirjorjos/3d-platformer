using Godot;
using System;

public partial class Player : CharacterBody3D
{
	private const float Speed = 5.0f;
	private const float JumpVelocity = 4.5f;
	private const float MOUSE_SPEED_V = 0.01f;
	private const float MOUSE_SPEED_H = 0.01f;
	private const float BUTTON_SPEED_V = 0.1f;
	private const float BUTTON_SPEED_H = 0.1f;
	private float _pitch = 0.0f;
	private Node3D neck;
	private Camera3D camera;

	public override void _Ready() {
		neck = GetNode<Node3D>("Neck");
		camera = GetNode<Camera3D>("Neck/Camera3D");
	}

	public override void _PhysicsProcess(double delta) {
		if (Input.IsActionJustPressed("Quit")) GetTree().Quit();
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor()) {
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("Jump") && IsOnFloor()) {
			velocity.Y = JumpVelocity;
		}

		// Handle Movement
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForwards", "MoveBackwards");
		Vector3 inputDirection = (neck.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		float inputStrength = inputDir.Length(); // analog controller stick support
		inputDirection *= Mathf.Min(inputStrength, 1.0f);
		if (inputDirection != Vector3.Zero) {
			velocity.X = inputDirection.X * Speed;
			velocity.Z = inputDirection.Z * Speed;
		} else {
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		// Handle Camera
		Vector2 cameraDir = Input.GetVector("LookRight", "LookLeft", "LookDown", "LookUp");
		Vector3 cameraDirection = (Transform.Basis * new Vector3(-cameraDir.X, -cameraDir.Y, 0)).Normalized();
		float cameraStrength = cameraDir.Length();
		cameraDirection *= Mathf.Min(cameraStrength, 1.0f);
		Vector2 finalDir = new Vector2(cameraDirection.X, cameraDirection.Y);
		RotateCameraH(finalDir, BUTTON_SPEED_H);
		RotateCameraV(finalDir, BUTTON_SPEED_V);

		Velocity = velocity;
		MoveAndSlide();
	}

	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseButton) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
		} else if (@event.IsActionPressed("Pause")) {
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}

		if (Input.MouseMode == Input.MouseModeEnum.Captured && @event is InputEventMouseMotion mouseMotionEvent) {
			if (camera == null || neck == null) return;
			RotateCameraH(mouseMotionEvent.Relative, MOUSE_SPEED_H);
			RotateCameraV(mouseMotionEvent.Relative, MOUSE_SPEED_V);
		}
	}

	private void RotateCameraH(Vector2 movement, float speed) {
		float rotation = movement.Y * speed;
		_pitch -= rotation;
		_pitch = Mathf.Clamp(_pitch, Mathf.DegToRad(-30), Mathf.DegToRad(60));
		camera.Rotation = new Vector3(_pitch, 0, 0);
	}

	private void RotateCameraV(Vector2 movement, float speed) {
		float rotation = movement.X * speed;
		neck.RotateY(-rotation);
	}
}
