using Godot;
using System;

public partial class LeftZone : Control
{
	
	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventScreenTouch touch)
		{
			if (touch.Pressed)
				Input.ActionPress("left_touch");
			else
				Input.ActionRelease("left_touch");
		}
	}

}
