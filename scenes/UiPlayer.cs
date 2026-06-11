using Godot;
using System;

public partial class UiPlayer : CanvasLayer
{
    
    [Export] Label Distancelabel;
    [Export] Label goldLabel;




    public void RefreshLabel(string labelName, string val)
    {
        switch (labelName)
        {
            case "Distance":
                Distancelabel.Text = val;
                break;
            case "Gold":
                goldLabel.Text = val;
                break;
        }
    }


    private void _on_restart_pressed()
    {
        GetTree().ReloadCurrentScene();
    }


    private void _on_exitmenu_pressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/menus.tscn");
	}

}
