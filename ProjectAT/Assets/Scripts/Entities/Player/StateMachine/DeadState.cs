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

        public override void OnExit()
        {
            Debug.Log("Exited Dead State");
        }

        public override void OnUpdate()
        {
            
        }
    }
}
