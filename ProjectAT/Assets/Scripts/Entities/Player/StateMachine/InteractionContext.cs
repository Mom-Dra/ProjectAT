using UnityEngine;
using System;

namespace PlayerStateMachine
{
    public enum PlayerStateType
    {
        None = -1,
        Normal,
        Dead,
        SkillChase,
        SkillCast,
        Interacting,
        Carry,

    }

    public struct InteractionContext 
    {
        public string animationTrigger; // 재생할 애니메이션 이름
        public float duration;          // 소요 시간
        public PlayerStateType nextStateOnSuccess; // 성공 시 전이할 상태 (Carry 또는 Normal)
        public Action onExecute;        // 실제 로직 (시체 부착 또는 분리)
    }
} 
