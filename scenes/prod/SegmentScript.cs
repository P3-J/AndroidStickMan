using Godot;
using System;

public partial class SegmentScript : Node3D
{
    
    [Export] float length;
    public float endingPosZ;


    public float GetLength()
    {
        return length;
    }



}
