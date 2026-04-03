using UnityEngine;


namespace PlayerStateMachine
{
    public class DeadState : PlayerState
    {
        public DeadState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            Debug.Log("Entered Dead State");
        }

        public override void OnUpdate()
        {
            // 사망 상태에서 필요한 업데이트 로직 작성
        }

        public override void OnExit()
        {
            Debug.Log("Exited Dead State");
        }
    }
}
