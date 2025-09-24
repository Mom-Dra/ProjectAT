
public abstract class EntityState
{
    protected PlayerStateMachine context;
    
    protected EntityState(PlayerStateMachine cxt) { context = cxt; }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void OnUpdate();
    public abstract void HandleInput(PlayerInputType type);
}
