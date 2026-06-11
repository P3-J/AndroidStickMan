using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;



public partial class Animroot : Node3D
{
    
    [Export] Camera3D maincam;

    Vector3 mainCamStartRot;

    Tween mainTween;

    Dictionary<string, float> rots = new Dictionary<string, float>
    {
      {"mainview", -90f},  
      {"rooftoplevel", -160f},  
      {"underground", -170f},  
      {"customize", 26f},  
    };



    public override void _Ready()
    {
        base._Ready();
        mainCamStartRot = maincam.GlobalRotation;
    }


    public void RotateCamTo(string viewName, float fov)
    {
        if (mainTween != null) mainTween.Kill();
        mainTween = GetTree().CreateTween();
        mainTween.TweenProperty(maincam, "rotation_degrees", new Vector3(0, rots[viewName], 0), 1)
        .SetTrans(Tween.TransitionType.Sine)
         .SetEase(Tween.EaseType.InOut);
        
        mainTween.TweenProperty(maincam, "fov", fov, 1)
        .SetTrans(Tween.TransitionType.Sine)
         .SetEase(Tween.EaseType.InOut);
        
    }


}
