using Unity.VisualScripting;
using UnityEngine;

public abstract class EntityState
{
    protected PlayerStateMachine context;
    public abstract void Enter();
    public abstract void Exit();
    public abstract void OnUpdate();
    public abstract void HandleClickInput();
}
