using UnityEngine;
using PlayerStatusCapabilities;


namespace PlayerStateMachine
{
    public class InteractingState : PlayerState, IRightClickHandler
    {
        public InteractingState(PlayerController context) : base(context) { }

        public override void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExit()
        {
            throw new System.NotImplementedException();
        }

        public override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public void OnRightClick(RaycastHit castedObject)
        {
            //TODO : 상호작용 중 우클릭 시 해당 위치를 캔슬, 노멀 State로 돌아가기 및 이동 
        }
    }
}
