using UnityEngine;


namespace PlayerStateMachine
{
    public class DeadState : PlayerState
    {
        private EntityStatus playerStatus;
        public DeadState(PlayerController context) : base(context)
        {
            playerStatus = context.MyStatus;
        }

        public override void OnEnter()
        {
            //status의 die함수는 여기에다 추가하는걸로?
        }

        public override void OnExit()
        {
            Debug.Log("Exited Dead State");
        }

        public override void OnUpdate()
        {
            
        }
    }
}
