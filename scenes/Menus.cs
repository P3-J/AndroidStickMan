using Godot;
using System;
using System.Collections.Generic;

public partial class Menus : Node3D
{


    private Dictionary<int, string> levels = new()
    {
        [0] = "res://scenes/world1.tscn",
        [1] = "res://scenes/world_2.tscn",
        [2] = "res://scenes/prod/infinity_world.tscn",
    };


    private void _on_item_list_item_activated(int index)
    {
        
        GetTree().ChangeSceneToFile(levels[index]);

    }


}
