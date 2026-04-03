using UnityEngine;


namespace PlayerStateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate();
        void OnExit();
    }
}