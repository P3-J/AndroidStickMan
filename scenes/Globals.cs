using Godot;
using System;

public partial class Globals : Node
{
	
	[Signal] public delegate void CoinPickedUpEventHandler();
	[Signal] public delegate void PlayerInGrindAreaEventHandler();
	[Signal] public delegate void PlayerInDamageAreaEventHandler();

	[Signal] public delegate void ZoneEndTriggerEventHandler();
}
