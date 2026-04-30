using Godot;

public partial class RunnerPlayer : Player {
	private const float MAX_YAW_DEG = 45;
	private const float SPEED_X = 0.2f;
	private PlatformLogic platformLogic;
	private float accumulatedYaw;
	private float neckYaw;

	public override void _Ready() {
		base._Ready();
		platformLogic = GetNode<PlatformLogic>("/root/PlatformLogic");
		neckYaw = neck.Rotation.Y;
		accumulatedYaw = neckYaw;
		pitch = camera.Rotation.X;
	}

	protected override Vector3 HandleActiveMovement(Vector3 velocity, double delta) {
		Vector2 inputDir = Input.GetVector("MoveLeft", "MoveRight", "MoveForwards", "MoveBackwards");
		float inputStrength = inputDir.Length();

		float moveSpeed = Speed * (float)delta;
		float strafe = inputDir.X * SPEED_X;

		Vector3 forwardDir = neck.GlobalTransform.Basis * Vector3.Back;
		Vector3 platformMovement = forwardDir * moveSpeed + new Vector3(strafe, 0, 0);

		platformLogic.MovePlatforms(platformMovement, this, Level, CanMovePlatforms());

		velocity.Z = strafe;
		return velocity;
	}

	protected override Vector3 ProcessPassiveMovement(Vector3 velocity, double delta) {
		if (!IsOnFloor()) {
			velocity += GetGravity() * (float)delta;
		} else if (!hasLanded) {
			hasLanded = true;
		}
		UpdateScore(1);
		return velocity;
	}

	public void UpdateScore(int addition) {
		platformLogic.UpdateScore(addition);
		Level.GetNode<Label>("CanvasLayer/Label").Text = $"{platformLogic.GetScore()}";
	}

	protected override void RotateCameraH(Vector2 movement, float speed) {
        float rotation = movement.X * speed;
        float centerYaw = neckYaw;
        float newYaw = accumulatedYaw + rotation;
        newYaw = Mathf.Clamp(newYaw, centerYaw - Mathf.DegToRad(MAX_YAW_DEG), centerYaw + Mathf.DegToRad(MAX_YAW_DEG));
        float actualRotation = newYaw - accumulatedYaw;
        accumulatedYaw = newYaw;

        neck.RotateY(actualRotation);
        RotateY(actualRotation);
    }
}
