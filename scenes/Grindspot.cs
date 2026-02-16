using Godot;
using System;

public partial class Grindspot : Area3D
{
    

    Globals glob;


    public override void _Ready()
    {
        base._Ready();

        glob = GetNode<Globals>("/root/Globals");

        BodyEntered += Entered;
        BodyExited += Exited;

    }



    private void Entered(Node3D body){
        GD.Print("inarea");
        if (body.IsInGroup("player")) glob.EmitSignal("PlayerInGrindArea", true);
    }

    private void Exited(Node3D body){
        if (body.IsInGroup("player")) glob.EmitSignal("PlayerInGrindArea", false);
    }


}
