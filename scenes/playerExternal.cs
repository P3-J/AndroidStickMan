using Godot;
using System;
using System.Data;

public partial class Player
{


    private Timer SpeedControllerTimer;
    
    private void SetupSpeedController()
    {
        
        if (SpeedControllerTimer != null) return;

        SpeedControllerTimer = new Timer
        {
            OneShot = false,
            WaitTime = 2.0f,
            Autostart = true,
        }; 

        SpeedControllerTimer.Timeout += IncreaseSpeed;
        AddChild(SpeedControllerTimer);
    }


    private void IncreaseSpeed()
    {
        return;
        ForwardSpeed += 1f;
        MaxOllieVelocity += 1f;

    }

}