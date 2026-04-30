using Godot;

public partial class Player : CharacterBody3D {
	protected const float Speed = 4.0f;
	private const float JUMP_VELOCITY = 6.5f;
	private const float BUTTON_SPEED_V = 0.1f;
	private const float BUTTON_SPEED_H = 0.01f;
	private const float MAX_PITCH_DEG = 45;
	private const float MIN_PITCH_DEG = 70;
	private const float MOUSE_SPEED_V = 0.01f;
	private const float MOUSE_SPEED_H = 0.001f;
	protected float pitch = 0.0f;
	private Node3D level;
	protected int score;
	public Node3D Level {
		get { return level; }
		protected set { level = value; }
	}
	protected Node3D neck;
	protected Camera3D camera;

	public bool isDead = false;
	public bool hasLanded = false;

	public bool CanMovePlatforms() {
		return hasLanded && !isDead;
	}

	public override void _Ready() {
		neck = GetNode<Node3D>("Neck");
		camera = GetNode<Camera3D>("Neck/Camera3D");
		level = GetNode<Node3D>("..");
	}
	public override void _UnhandledInput(InputEvent @event) {
		if (@event is InputEventMouseButton) {
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}

		if (Input.MouseMode == Input.MouseModeEnum.Captured && @event is InputEventMouseMotion mouseMotionEvent) {
			if (camera == null || neck == null) return;
			RotateCameraH(-mouseMotionEvent.Relative, MOUSE_SPEED_H);
			RotateCameraV(-mouseMotionEvent.Relative, MOUSE_SPEED_V);
		}
	}

	

	public override void _PhysicsProcess(double delta) {
		Vector3 velocity = Velocity;

		velocity = ProcessPassiveMovement(velocity, delta);
		velocity = HandleJump(velocity);
		velocity = HandleActiveMovement(velocity, delta);
		HandleMiscInputs();
		ProcessCameraMovement();
		Velocity = velocity;
		MoveAndSlide();
	}

	protected virtual Vector3 ProcessPassiveMovement(Vector3 velocity, double delta) {
		if (!IsOnFloor()) {
			velocity += GetGravity() * (float)delta;
		} else if (!hasLanded) {
			hasLanded = true;
		}
		return velocity;
	}

	protected Vector3 HandleJump(Vector3 velocity) {
		if (Input.IsActionJustPressed("Jump") && IsOnFloor()) {
			velocity.Y = JUMP_VELOCITY;
		}
		return velocity;
	}

	protected virtual Vector3 HandleActiveMovement(Vector3 velocity, double delta) {
		throw new System.Exception("Base player class being used");
	}

	private void HandleMiscInputs() {
		if (Input.IsActionJustPressed("Quit")) GetTree().Quit();
		if (Input.IsActionJustPressed("Pause")) Input.MouseMode = Input.MouseModeEnum.Visible;
	}

	protected void ProcessCameraMovement() {
		Vector2 cameraDir = Input.GetVector("LookRight", "LookLeft", "LookDown", "LookUp");
		float cameraStrength = cameraDir.Length();
		if (cameraStrength > 0.01f) {
			RotateCameraH(cameraDir, BUTTON_SPEED_H);
			RotateCameraV(cameraDir, BUTTON_SPEED_V);
		}
	}

	protected virtual void RotateCameraH(Vector2 movement, float speed) {
		float rotation = movement.X * speed;
		float currentYaw = -neck.Rotation.Y;
		float newYaw = currentYaw + rotation;
		float actualRotation = newYaw - currentYaw;
		neck.RotateY(actualRotation);
		RotateY(actualRotation);
	}

	protected virtual void RotateCameraV(Vector2 movement, float speed) {
		pitch += movement.Y * speed;
		pitch = Mathf.Clamp(pitch, -Mathf.DegToRad(MAX_PITCH_DEG), Mathf.DegToRad(MIN_PITCH_DEG));
		camera.Rotation = new Vector3(pitch, 0, 0);
	}
}