using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.AI;

public class PlayerStateMachine
{
    [SerializeField] private PlayerController myController;
    [SerializeField] public NavMeshAgent Agent { get { return myController.MyAgent; } }
    [SerializeField] public EntityStatus Status { get { return myController.MyStatus; } }

    public EntityState CurrentState { get; private set; }
    public IdleState IdleState { get; private set; }
    public WalkState WalkState { get; private set; }
    public RunState RunState { get; private set; }

    public PlayerStateMachine(PlayerController myController)
    {
        this.myController = myController;
        StateInitialize();
    }

    public void StateInitialize()
    {
        CurrentState = IdleState = new IdleState(this);
        WalkState = new WalkState(this);
        RunState = new RunState(this);

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

    public bool IsClickSamePosition()
    {
        Vector3 pos = myController.GetMouseWorldPosition();
        Vector3 dest = myController.MyAgent.destination;

        return pos.x == dest.x && pos.z == dest.z && (Mathf.Abs(pos.y - dest.y) <= 0.1);
    }

    public void PlayerMove()
    {
        myController.PlayerMove();
    }
}
