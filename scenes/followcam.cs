using Godot;
using System;



public partial class followcam : Camera3D
{
	

	[Export] Marker3D targetCamLocation;

	[Export] public float FollowSpeed = 0.5f;

	public override void _Ready()
	{
		ProcessPriority = 1000;
	}

	public override void _Process(double delta)
	{
		base._Process(delta);


		if (targetCamLocation == null) return;

		float t = 1f - Mathf.Exp(-FollowSpeed * (float)delta);

		GlobalPosition = GlobalPosition.Lerp(targetCamLocation.GlobalPosition, t);

	}



}
