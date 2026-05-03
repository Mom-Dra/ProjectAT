using PlayerStatusCapabilities;
using UnityEngine;

namespace PlayerStateMachine
{
    public class CarryState : PlayerState, IRightClickHandler
    {
        private PlayerInteractionModule myInteractionModule;

        public CarryState(PlayerController playerController) : base(playerController)
        {
            myInteractionModule = context.MyInteractionModule;
        }

        public override void OnEnter()
        {
            
        }

        public override void OnExit()
        {
            
        }

        public override void OnUpdate()
        {
            
        }

        public void OnRightClick(RaycastHit castedObject)
        {
            throw new System.NotImplementedException();
        }

    }
}