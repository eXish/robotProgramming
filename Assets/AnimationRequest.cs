using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRequest {
    public int RobotIndex;
    public robotProgramming.Direction Direction;
    public bool Crashed;
    public bool OOB;

    public AnimationRequest()
    {
        RobotIndex = 0;
        Direction = robotProgramming.Direction.Up;
        Crashed = false;
        OOB = false;
    }

    public AnimationRequest(int index, robotProgramming.Direction dir, bool crash, bool oob)
    {
        RobotIndex = index;
        Direction = dir;
        Crashed = crash;
        OOB = oob;
    }
}
