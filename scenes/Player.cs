using Godot;
using System;
using System.Data;

public partial class Player : CharacterBody3D
{

	[Export] Node3D stickRoot;
	[Export] Node3D modelroot;
	[Export] AnimationPlayer boardAnim;
	[Export] AudioStreamPlayer coinSound;
	[Export] AnimationPlayer stickmananim;
	[Export] Label distancelabel;
	Globals glob;
	bool firstTouchOff = false;
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
			PlayFlipAnim(false);
		}

		_canSwipe = false;
	}


	private bool IsTouchWithinValidHorizontalArea(Vector2 position)
	{
		float screenWidth = GetViewport().GetVisibleRect().Size.X;
		return position.X > HorizontalMargin && position.X < screenWidth - HorizontalMargin;
	}

	private float CalculateOlliePower(float swipeDistance)
	{
		// Clamp swipe distance between min/max thresholds
		float clampedDistance = Mathf.Clamp(swipeDistance, MinOllieSwipeDistance, MaxOllieSwipeDistance);

		// Normalize to 0.0 (min power) → 1.0 (max power)
		return Mathf.InverseLerp(MinOllieSwipeDistance, MaxOllieSwipeDistance, clampedDistance);
	}

	private void ApplyOllie(float power)
	{
		// Scale vertical velocity based on swipe power (0.0 → 1.0)
		float verticalVelocity = Mathf.Lerp(MinOllieVelocity, MaxOllieVelocity, power);

		Velocity = new Vector3(
			Velocity.X,
			verticalVelocity,
			Velocity.Z
		);

		PlayJumpAnim();
	}

	private void ApplyBounce()
	{
		Velocity = new Vector3(Velocity.X, -downwardsSpeed, Velocity.Z);
	}


	public override void _Process(double delta)
	{
		base._Process(delta);
		float pz = GlobalPosition.Z;
		pz = Mathf.Round(pz);

		if (pz > 0)
		{
			distancelabel.Text = "DISTANCE: " + pz;
		}
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

			if (firstTouchOff)
			{
				firstTouchOff = false;
				stickmananim.PlayBackwards("jump");
			}

			Vector3 floorNormal = GetFloorNormal();
			// The steeper the slope downward, the more we pull down
			// flat ground: floorNormal.Y = 1 → no extra pull
			// steep downhill: floorNormal.Y < 1 → apply extra downward acceleration
			float slopeFactor = Mathf.Clamp(1.0f - floorNormal.Y, 0.0f, 1.0f);
			float extraDownhillPull = 30.0f;
			velocity.Y -= extraDownhillPull * slopeFactor * (float)delta;

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
		if (stickmananim.IsPlaying()) stickmananim.Play("RESET");
		stickmananim.Play("jump");
		firstTouchOff = true;
	}


	private void PickedUpCoinAction()
	{
		coinSound.Play();
	}

	private void PlayFlipAnim(bool state)
	{

		GD.Randomize();
		Random rand = new Random();

		int flipValue = rand.Next(3);
		boardAnim.SpeedScale = 1;
		stickmananim.SpeedScale = 0.5f;

		bool AnimPlaying = boardAnim.IsPlaying();

		if (!state && boardAnim.IsPlaying())
		{
			boardAnim.SpeedScale = 2;
			stickmananim.SpeedScale = 2;
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

	private void _on_button_pressed()
	{
		GetTree().ReloadCurrentScene();
	}

	private void GrindAreaReceive(bool state)
	{
		inGrindArea = state;
		if (state == true) stickmananim.Play("stand");
		GD.Print("got it wtf");
	}
	private void ZoneEndReceive()
	{
		inGrindArea = true; // trigger break
		inEndZone = true;

	}

	private void DamageAreaReceive(bool state)
	{
		_on_button_pressed();

	}



	private void _on_exitmenu_pressed()
	{

		GetTree().ChangeSceneToFile("res://scenes/menus.tscn");
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
