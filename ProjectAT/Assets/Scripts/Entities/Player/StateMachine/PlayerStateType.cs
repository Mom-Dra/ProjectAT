using UnityEngine;
using System;

namespace PlayerStateMachine
{
    public enum PlayerStateType //Enemy도 이걸 사용할 걸 고려하면 다른 이름 채택하기. 그냥 EntityStateType같은거로
    {
        None = -1,
        Normal,
        Dead,
        SkillChase,
        SkillCast,
        SkillExecute,
        InteractChasing,
        Interacting,
        Carry,
        Cover,

    }
} 
