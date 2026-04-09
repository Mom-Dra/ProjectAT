using UnityEngine;
using PlayerStateMachine;

public abstract class PlayerState : IState
{
    protected PlayerController context;

    public PlayerState(PlayerController playerController)
    {
        context = playerController;
    }

    public abstract void OnEnter();
    public abstract void OnUpdate();
    public abstract void OnExit();    
}
