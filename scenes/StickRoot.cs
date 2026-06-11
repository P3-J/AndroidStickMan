using Godot;
using System;

public partial class StickRoot : Node3D
{
    


    [Export] AnimationPlayer stickManAnim;

    public override void _Ready()
    {
        base._Ready();
    }



    public void PlayAnim(string AnimName)
    {
        stickManAnim.Play(AnimName);
    }

    public void StopAnim(string AnimName)
    {
        stickManAnim.Stop();
    }

    public void PlayAnimBackWards(string AnimName)
    {
        stickManAnim.PlayBackwards(AnimName);
    }

    public void ChangeAnimSpeed(float speed)
    {
        stickManAnim.SpeedScale = speed;
    }


    public void PlayJumpAnim()
	{
		if (stickManAnim.IsPlaying()) stickManAnim.Play("RESET");
		stickManAnim.Play("jump");
	}


    //////////////// HAT STUFF
    /// 
    /// 
    
    [Export] PackedScene strawHatScene;
    [Export] BoneAttachment3D hatBone;
    [Export] Marker3D hatMarker;

    MeshInstance3D currentHat;

    public void SpawnHatOnHead(string hatName)
    {
        currentHat?.QueueFree();
        currentHat = null;

        if (hatName == "default") return;

        PackedScene cScene;


        switch (hatName)
        {
            case "strawhat":
                cScene = strawHatScene;
                break;
            default:
                return;
        }

        MeshInstance3D newHat = cScene.Instantiate<MeshInstance3D>();
        hatBone.AddChild(newHat);
        newHat.GlobalPosition = hatMarker.GlobalPosition;
        currentHat = newHat;

    }


}
