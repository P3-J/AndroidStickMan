using Godot;
using System;
using System.Data;

public partial class Player : CharacterBody3D
{

	[Export] Node3D modelroot;
	[Export] AnimationPlayer boardAnim;
	[Export] AudioStreamPlayer coinSound;
	[Export] AudioStreamPlayer grindSound;
	[Export] AudioStreamPlayer rollingSound;
	[Export] AudioStreamPlayer ollieSound;
	[Export] StickRoot stickRoot;
	[Export] UiPlayer UI;
	Globals glob;
	public bool firstTouchOff = false;
	bool inEndZone = false;
	private Vector2 _touchStartPos;
	private bool _canSwipe = false;

	bool inGrindArea = false;
	private float _gravity = 9.91f;


	public override void _Ready()
	{
		base._Ready();
		glob = GetNode<Globals>("/root/Globals");

		glob.Connect("CoinPickedUp", new Callable(this, nameof(PickedUpCoinAction)));
		glob.Connect("PlayerInGrindArea", new Callable(this, nameof(GrindAreaReceive)));
		glob.Connect("ZoneEndTrigger", new Callable(this, nameof(ZoneEndReceive)));
		glob.Connect("PlayerInDamageArea", new Callable(this, nameof(DamageAreaReceive)));

		SetupEquipedItems();
		SetupSpeedController();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventScreenTouch touchEvent)
			return;

		if (touchEvent.Pressed)
		{
			HandleTouchPress(touchEvent);
		}
		else
		{
			HandleTouchRelease(touchEvent);
		}
	}

	private void HandleTouchPress(InputEventScreenTouch touch)
	{
		_touchStartPos = touch.Position;
		_canSwipe = IsTouchWithinValidHorizontalArea(touch.Position);
	}

	private void HandleTouchRelease(InputEventScreenTouch touch)
	{
		if (!_canSwipe)
			return;

		float swipeDistance = _touchStartPos.Y - touch.Position.Y;


		if (swipeDistance >= MinOllieSwipeDistance && IsOnFloor())
		{
			float olliePower = CalculateOlliePower(swipeDistance);
			ApplyOllie(olliePower);
			PlayFlipAnim(true);
		}

		else if (swipeDistance <= -MinBounceSwipeDistance)
		{
			ApplyBounce();
		}

		_canSwipe = false;
	}


	private bool IsTouchWithinValidHorizontalArea(Vector2 position)
	{
		float screenWidth = GetViewport().GetVisibleRect().Size.X;
		return position.X > HorizontalMargin && position.X < screenWidth - HorizontalMargin;
	}

	private void SetupEquipedItems()
	{
		stickRoot.SpawnHatOnHead(glob.equipedItems["head"]);
	}

	private float CalculateOlliePower(float swipeDistance)
	{
		float clampedDistance = Mathf.Clamp(swipeDistance, MinOllieSwipeDistance, MaxOllieSwipeDistance);
		return Mathf.InverseLerp(MinOllieSwipeDistance, MaxOllieSwipeDistance, clampedDistance);
	}

	private void ApplyOllie(float power)
	{
		float verticalVelocity = Mathf.Lerp(MinOllieVelocity, MaxOllieVelocity, power);

		Velocity = new Vector3(
			Velocity.X,
			verticalVelocity,
			Velocity.Z
		);

		PlayJumpAnim();
		PlaySound("rolling", false);
		PlaySound("ollie");
	}

	private void ApplyBounce()
	{
		Velocity = new Vector3(
			Velocity.X,
			Velocity.Y - downwardsSpeed,
			Velocity.Z
		);

	}


	public override void _Process(double delta)
	{
		base._Process(delta);
		float pz = GlobalPosition.Z;
		pz = Mathf.Round(pz);

		if (IsOnFloor())
		{
			PlaySound("rolling");
		} else
		{
			PlaySound("rolling", false);
		}

		if (pz > 0) UI.RefreshLabel("Distance", "DISTANCE: " + pz);
		
	}


	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		if (!IsOnFloor())
		{
			if (velocity.Y > 0)
			{
				// Rising (just jumped)
				velocity.Y -= _jumpGravity * (float)delta;
			}
			else
			{
				// Falling (coming down)
				velocity.Y -= _fallGravity * (float)delta;
			}
		}
		else
		{
			if (!firstTouchOff) PlayFlipAnim(false);
			if (firstTouchOff)
			{
				firstTouchOff = false;
				stickRoot.PlayAnimBackWards("jump");
			}
			//PlayFlipAnim(false);
		}


		float moveDir = 0.0f;

		bool rightPressed = Input.IsActionPressed("right_touch");
		bool leftPressed = Input.IsActionPressed("left_touch");



		if (rightPressed)
		{
			moveDir -= 0.5f;
			stickRoot.RotationDegrees = new Vector3(10, stickRoot.Rotation.Y, stickRoot.Rotation.Z);
		}
		else if (leftPressed)
		{
			moveDir += 0.5f;
			stickRoot.RotationDegrees = new Vector3(-10, stickRoot.Rotation.Y, stickRoot.Rotation.Z);
		}
		else
		{
			stickRoot.RotationDegrees = new Vector3(0, stickRoot.Rotation.Y, stickRoot.Rotation.Z);
		}



		if (!inEndZone)
		{
			velocity.X = Mathf.Lerp(velocity.X, moveDir * SideSpeed, 0.1f);
			velocity.Z = ForwardSpeed;
		}
		else
		{
			velocity = velocity.MoveToward(Vector3.Zero, 10f * (float)delta);
		}

		Velocity = velocity;
		MoveAndSlide();
		GroundNormalRotBody(delta);



	}

	private void PlaySound(string soundName, bool state = true)
	{
		
		switch (soundName)
		{
			
			case "ollie":
                _soundManager(ollieSound, state);
                break;
			case "rolling":
				_soundManager(rollingSound, state);
				break;
			case "grind":
				_soundManager(grindSound, state);
				break;

		}

	}


	private void _soundManager(AudioStreamPlayer audio, bool state)
	{
		if (!audio.Playing && state) {audio.Play();}
		else if (!state) {audio.Stop();}
	}

	private void GroundNormalRotBody(double delta)
	{

		if (!IsOnFloor())
		{
			modelroot.GlobalBasis = modelroot.GlobalBasis.Slerp(
			Basis.FromEuler(new Vector3(0, Rotation.Y, 0)),
			12f * (float)delta
			);
			return;
		}

		if (inGrindArea)
		{
			modelroot.GlobalBasis = modelroot.GlobalBasis.Slerp(
			Basis.FromEuler(new Vector3(0, 110, 0)),
			24f * (float)delta
			);
			return;
		} 

		Vector3 up = GetFloorNormal().Normalized();

		Basis bodyYaw = Basis.FromEuler(new Vector3(0, Rotation.Y, 0));
		Vector3 forward = -bodyYaw.Z;
		forward = forward.Slide(up);

		if (forward.LengthSquared() < 0.0001f)
			return;

		forward = forward.Normalized();

		Basis slopeBasis = new Basis(
			forward.Cross(up).Normalized(),
			up,
			-forward
		).Orthonormalized();

		Basis target = slopeBasis;
		modelroot.GlobalBasis = modelroot.GlobalBasis.Slerp(
			target,
			12f * (float)delta
		);
	}


	private void PlayJumpAnim()
	{
		stickRoot.PlayJumpAnim();
		firstTouchOff = true;
	}


	private void PickedUpCoinAction()
	{
		coinSound.Play();
		glob.playerGold += 1;
		UI.RefreshLabel("Gold", "Gold " + glob.playerGold.ToString());
	}

	private void PlayFlipAnim(bool state)
	{

		GD.Randomize();
		Random rand = new Random();

		int flipValue = rand.Next(3);
		boardAnim.SpeedScale = 1;
		stickRoot.ChangeAnimSpeed(0.5f);

		bool AnimPlaying = boardAnim.IsPlaying();

		if (!state && boardAnim.IsPlaying())
		{
			boardAnim.SpeedScale = 2;
			stickRoot.ChangeAnimSpeed(3f);
			return;
		}


		if (AnimPlaying || !state) return;

		switch (flipValue)
		{
			case 0:
				boardAnim.Play("kickflip");
				break;
			case 1:
				boardAnim.Play("shuvit");
				break;
			case 2:
				boardAnim.Play("treflip");
				break;
		}


	}

	private void GrindAreaReceive(bool state)
	{
		inGrindArea = state;
		if (state == true) stickRoot.PlayAnim("stand");
		
		PlaySound("grind", state);
	}
	private void ZoneEndReceive()
	{
		inGrindArea = true; // trigger break
		inEndZone = true;

	}

	private void DamageAreaReceive(bool state)
	{
		CallDeferred("reload_current_scene");
	}

	public void WallRideLean(string dir, bool state = true)
	{
		// no for now
		return;
		if (!state) {
			stickRoot.StopAnim("wallRight");
			boardAnim.Play("RESET");
			return;
		}
		GD.Print("lean");
		stickRoot.PlayAnim("wallRight");
		boardAnim.Play("rightWallride");

	}



	[Export] private float MinOllieSwipeDistance = 60f;   // Minimum swipe (pixels) to trigger ollie
	[Export] private float MaxOllieSwipeDistance = 250f;  // Swipe beyond this gives max power
	[Export] private float MinOllieVelocity = 15f;        // Minimum upward velocity
	[Export] private float MaxOllieVelocity = 45f;        // Maximum upward velocity
	[Export] private float MinBounceSwipeDistance = 90f;  // Downward swipe threshold
	[Export] private float HorizontalMargin = 0f;         // Disable swipes near screen edges
	[Export] public float ForwardSpeed = 10.0f;
	[Export] public float SideSpeed = 15.0f; // Speed of side-to-side movement
	[Export] public float downwardsSpeed = 6.0f;
	[Export] public float _jumpGravity = 5f;
	[Export] public float _fallGravity = 5f;


}
