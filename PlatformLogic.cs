using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlatformLogic : Node {

	private const float GENERATION_WIDTH = 15f;
	private const float CLEANUP_DISTANCE = 10f;
	private const int MIN_PLATFORM_Z_GAP = 2;
	private const int MAX_PLATFORM_Z_GAP = 3;
	private const int MIN_PLATFORM_Y_GAP = 0;
	private const int MAX_PLATFORM_Y_GAP = 2;
	private const int MAX_PLATFORM_X_GAP = 3;
	private const float MIN_X = -10f;
	private const float MAX_X = 10f;
	private const float MIN_Y = -20f;
	private const float MAX_Y = 20f;
	private const float MULTIPLE_PLATFORM_IN_LANE_CHANCE = 0.5f;

	private List<PackedScene> platforms = new List<PackedScene>();
	private List<Vector3> platformSizes = new List<Vector3>();
	private Lane[] lanes;
	private int laneCount;
	private float laneWidth;
	private float lastCleanupX;
	private float virtualPlayerX;
	private Random rng = new Random();
	private bool firstSpawnDone = false;

	private class Lane {
		public Vector3 nextSpawnPos;
		public Vector3 currentPlatformSize;
		public float currentYBottom;
		public float currentYTop;
	}

	public override void _Ready() {
		string[] platformDirFiles = DirAccess.GetFilesAt("./asset_scenes/Platforms");
		platforms.AddRange(
			platformDirFiles.Select(
				file => (PackedScene)GD.Load($"res://asset_scenes/Platforms/{file}")
			)
		);

		foreach (var scene in platforms) {
			StaticBody3D instance = scene.Instantiate<StaticBody3D>();
			CollisionShape3D col = instance.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
			float x = (col?.Shape as BoxShape3D)?.Size.X ?? 0f;
			float z = (col?.Shape as BoxShape3D)?.Size.Z ?? 0f;
			float y = (col?.Shape as BoxShape3D)?.Size.Y ?? 0f;
			platformSizes.Add(new Vector3(x, y, z));
			instance.QueueFree();
		}

		float maxPlatformDim = platformSizes.Max(p => Math.Max(p.X, p.Z));
		laneWidth = maxPlatformDim / 2f + MAX_PLATFORM_X_GAP + maxPlatformDim / 2f;
		laneCount = (int)Math.Ceiling((MAX_X - MIN_X) / laneWidth);
		lanes = new Lane[laneCount];

		for (int i = 0; i < laneCount; i++) {
			lanes[i] = new Lane {
				nextSpawnPos = new Vector3(MIN_X + (i + 0.5f) * laneWidth, 0, 0),
				currentPlatformSize = Vector3.Zero,
				currentYBottom = -1000f,
				currentYTop = 1000f
			};
		}

		lastCleanupX = -1000f;
	}

	public void MovePlatforms(Vector3 movement, Player player, Node scene, bool shouldMove = true) {
		if (movement.X == 0 && movement.Y == 0 && movement.Z == 0) {
			GeneratePlatform(scene, player);
			CleanBehind(scene);
			return;
		}

		if (shouldMove) {
			Vector3 platformMovement = new Vector3(movement.X, movement.Y, movement.Z);

			foreach (Node child in scene.GetChildren()) {
				if (child is StaticBody3D platform) {
					platform.GlobalPosition += platformMovement;
					platform.GlobalPosition = new Vector3(
						platform.GlobalPosition.X,
						Mathf.Clamp(platform.GlobalPosition.Y, MIN_Y, MAX_Y),
						platform.GlobalPosition.Z
					);
				}
			}

			if (player.isDead || player.GlobalPosition.Y < (MIN_Y - 10f)) {
				if (!player.isDead) {
					GD.Print("Player fell below death plane");
					player.isDead = true;
				}
				return;
			}

			for (int i = 0; i < laneCount; i++) {
				lanes[i].nextSpawnPos += platformMovement;
			}
		}

		GeneratePlatform(scene, player);
		CleanBehind(scene);
	}

	private void GeneratePlatform(Node scene, Player player) {
		float playerZ = player.GlobalPosition.Z;
		float spawnAheadZ = playerZ + GENERATION_WIDTH;

		bool anyLaneNeedsSpawn = false;
		for (int i = 0; i < laneCount; i++) {
			if (lanes[i].nextSpawnPos.Z < spawnAheadZ) {
				anyLaneNeedsSpawn = true;
				break;
			}
		}

		if (!anyLaneNeedsSpawn) return;

		bool isFirstSpawn = !firstSpawnDone && lanes.All(l => l.nextSpawnPos.Z == 0 && l.nextSpawnPos.Y == 0);

		if (isFirstSpawn) {
			firstSpawnDone = true;
			int startLane = rng.Next(laneCount);
			float nearestLaneX = lanes[startLane].nextSpawnPos.X;
			float playerXPos = player.GlobalPosition.X;
			float minDist = Math.Abs(playerXPos - nearestLaneX);

			for (int i = 0; i < laneCount; i++) {
				float laneX = lanes[i].nextSpawnPos.X;
				float dist = Math.Abs(playerXPos - laneX);
				if (dist < minDist) {
					minDist = dist;
					startLane = i;
				}
			}

			player.GlobalPosition = new Vector3(lanes[startLane].nextSpawnPos.X, player.GlobalPosition.Y, player.GlobalPosition.Z);
			lanes[startLane].nextSpawnPos = new Vector3(
				lanes[startLane].nextSpawnPos.X,
				-5f,
				0f
			);
			SpawnPlatform(lanes[startLane].nextSpawnPos, scene, -1, lanes[startLane]);
			return;
		}

		int laneToSpawn = rng.Next(laneCount);
		for (int lane = 0; lane < laneCount; lane++) {
			int currentLane = (laneToSpawn + lane) % laneCount;

			if (lanes[currentLane].nextSpawnPos.Z >= spawnAheadZ) continue;

			int nextPlatformIndex = rng.Next(platforms.Count);
			Vector3 nextPlatformSize = platformSizes[nextPlatformIndex];

			float currentZSize = lanes[currentLane].currentPlatformSize.Z;
			float nextZSize = nextPlatformSize.Z;
			float currentYSize = lanes[currentLane].currentPlatformSize.Y;
			float nextYSize = nextPlatformSize.Y;

			float zGap = rng.Next(MIN_PLATFORM_Z_GAP, MAX_PLATFORM_Z_GAP + 1);
			float zOffset = zGap + currentZSize / 2f + nextZSize / 2f;

			float yGap = rng.Next(MIN_PLATFORM_Y_GAP, MAX_PLATFORM_Y_GAP + 1);
			float minRequiredYShift = yGap + currentYSize / 2f + nextYSize / 2f;
			float yOffset = yGap < minRequiredYShift ? minRequiredYShift : yGap;

			Vector3 newPos = new Vector3(
				lanes[currentLane].nextSpawnPos.X,
				Mathf.Clamp(lanes[currentLane].nextSpawnPos.Y + yOffset, MIN_Y, MAX_Y),
				lanes[currentLane].nextSpawnPos.Z + zOffset
			);

			SpawnPlatform(newPos, scene, nextPlatformIndex, lanes[currentLane]);

			lanes[currentLane].nextSpawnPos = newPos;

			if (rng.NextDouble() > MULTIPLE_PLATFORM_IN_LANE_CHANCE) break;
		}
	}

	private void CleanBehind(Node scene) {
		float playerZ = scene.GetNode<Player>("RunnerPlayer").GlobalPosition.Z;
		float cleanupThreshold = playerZ - CLEANUP_DISTANCE;

		if (lastCleanupX < cleanupThreshold) {
			foreach (Node child in scene.GetChildren()) {
				if (child is StaticBody3D platform && platform.GlobalPosition.Z < cleanupThreshold) {
					platform.QueueFree();
				}
			}
			lastCleanupX = cleanupThreshold;
		}
	}

	private void SpawnPlatform(Vector3 pos, Node scene, int platformIndex = -1, Lane lane = null) {
		if (platformIndex < 0) platformIndex = rng.Next(platforms.Count);
		StaticBody3D newPlatform = platforms[platformIndex].Instantiate<StaticBody3D>();
		scene.AddChild(newPlatform);
		newPlatform.GlobalPosition = pos;

		int rotationSteps = rng.Next(4);
		if (rotationSteps > 0) {
			newPlatform.RotateY((float)(rotationSteps * Math.PI / 2f));
		}

		Vector3 platformSize = platformSizes[platformIndex];
		if (rotationSteps == 1 || rotationSteps == 3) {
			platformSize = new Vector3(platformSize.Z, platformSize.Y, platformSize.X);
		}

		if (lane != null) {
			lane.currentPlatformSize = platformSize;
			lane.currentYBottom = pos.Y - platformSize.Y / 2f;
			lane.currentYTop = pos.Y + platformSize.Y / 2f;
		}
	}

	public override void _Process(double delta) {
	}
}
