using Godot;

public partial class RunnerPlayer : Player {
	private const float MAX_YAW_DEG = 90;
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

		platformLogic.MovePlatforms(platformMovement, this, level, CanMovePlatforms());

		velocity.Z = strafe;
		return velocity;
	}

	protected override void RotateCameraH(Vector2 movement, float speed) {
		float rotation = movement.X * speed;
		accumulatedYaw += rotation;
		float centerYaw = neckYaw;
		accumulatedYaw = Mathf.Clamp(accumulatedYaw, centerYaw - Mathf.DegToRad(MAX_YAW_DEG), centerYaw + Mathf.DegToRad(MAX_YAW_DEG));
		float newYaw = accumulatedYaw;
		float actualRotation = newYaw - neckYaw;
		neckYaw = newYaw;
		neck.Rotation = new Vector3(neck.Rotation.X, newYaw, neck.Rotation.Z);
		RotateY(actualRotation);
	}
}
