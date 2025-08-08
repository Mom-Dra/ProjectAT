using UnityEngine;
using UnityEngine.AI;

public class PlayerStateMachine
{
    [SerializeField] private PlayerController myController;
    [SerializeField] public NavMeshAgent Agent { get { return myController.MyAgent; } }

    public EntityState CurrentState { get; private set; }
    public IdleState IdleState { get; private set; }
    public WalkState WalkState { get; private set; }

    public PlayerStateMachine(PlayerController myController)
    {
        this.myController = myController;
        StateInitialize();
    }

    public void StateInitialize()
    {
        CurrentState = IdleState = new IdleState(this);
        WalkState = new WalkState(this);
    }

    public void ChangeState(EntityState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void HandleClickInput()
    {
        CurrentState.HandleClickInput();
    }

    public void OnUpdate()
    {
        CurrentState.OnUpdate();
    }

    public bool IsArrivedToDest()
    {
        return Agent.destination == Agent.transform.position;
    }

    public void PlayerMove()
    {
        myController.PlayerMove();
    }
}
