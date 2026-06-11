using Godot;
using System;
using System.Collections.Generic;

public partial class Globals : Node
{
	
	[Signal] public delegate void CoinPickedUpEventHandler();
	[Signal] public delegate void PlayerInGrindAreaEventHandler();
	[Signal] public delegate void PlayerInDamageAreaEventHandler();
	[Signal] public delegate void ZoneEndTriggerEventHandler();


	public int playerGold {get; set;} = 50;

	public Dictionary<string, string> equipedItems = new()
	{
		["head"] = "default",
		["color"] = "cyan",
		["board"] = "default",
	};

	public Dictionary<string, Dictionary<string, Dictionary<string, Variant>>> allItems = new()
	{
		["head"] = new() {
			["default"] = new()
			{
				["owned"] = true,
				["price"] = 0,
			},
			["strawhat"] = new()
			{
				["owned"] = false,
				["price"] = 50,
			}
		},
		["color"] = new() {
			["cyan"] = new()
			{
				["owned"] = true,
				["price"] = 0,
			}
		},
		["board"] = new() {
			["default"] = new()
			{
				["owned"] = true,
				["price"] = 0,
			}
		}
	};


}
