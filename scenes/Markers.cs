using Godot;
using System;

public partial class Markers : Node3D
{
    

    [Export] RayCast3D rightCast;
    [Export] RayCast3D leftCast;

    [Export] Player player;

    private bool ridingWall = false;



    public override void _Process(double delta)
    {
        base._Process(delta);

        if (player.firstTouchOff == true) return;
        CheckForWallRideLean();

    }

    private void CheckForWallRideLean()
    {
        
        bool rCol = rightCast.IsColliding();
        bool lCol = leftCast.IsColliding();

        if (ridingWall)
        {

            if (!rCol && !lCol)
            {
                player.WallRideLean("x", false);
                ridingWall = false;
            }
            return;
        }
        if (rCol && lCol) return;


        RayCast3D collider = rCol ? rightCast : leftCast;
        GodotObject colliderObj = collider.GetCollider();

        if (colliderObj is StaticBody3D)
        {
            ridingWall = true;
            player.WallRideLean(rCol ? "right" : "left");

        }


    }

}
