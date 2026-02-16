using Godot;
using System;

public partial class Coin : Area3D
{
	
	Globals glob;


	public override void _Ready()
	{
		base._Ready();
		glob = GetNode<Globals>("/root/Globals");


	}


	private void _on_body_entered(Node3D body)
	{
		
		if (body.IsInGroup("player") && body is Player)
		{
			glob.EmitSignal("CoinPickedUp");
			CallDeferred("queue_free");
		}
		

	}


}
