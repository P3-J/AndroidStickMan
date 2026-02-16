using Godot;
using System;

public partial class RightZone : Control
{
	

	public override void _GuiInput(InputEvent @event)
	{
		if (@event is InputEventScreenTouch touch)
		{
			if (touch.Pressed)
				Input.ActionPress("right_touch");
			else
				Input.ActionRelease("right_touch");
		}
	}

}
