using UnityEngine;

public class PlayerChaseStateBase : EntityState
{
    private float range = 2.0f;
    private PlayerStateMachine.StateId nextState = PlayerStateMachine.StateId.Attack;

    public PlayerChaseStateBase(PlayerStateMachine cxt) : base(cxt) { }

    public override void Enter()
    { 
        context.PlayerController.MovePosition(context.PlayerController.SelectedEnemy.transform.position);
        context.PlayerController.MyAnim.SetBool("isWalking", true);
    }

    public override void Exit()
    {
        context.PlayerController.MyAnim.SetBool("isWalking", false);
    }

    public override void HandleInput(PlayerInputType type)
    {
        PlayerStateMachine.StateId nextState = context.PlayerController.CalCulateNextStateByMouseRaycast();
        if (nextState != PlayerStateMachine.StateId.Chase) //공격이 아니면
        {
            context.PlayerController.SetTargetEnemy(null); //타겟 초기화
            context.ChangeState(nextState);
        }
    }

    public override void OnUpdate()
    {
        if (context.PlayerController.IsInAttackRange(context.PlayerController.SelectedEnemy))
        {
            context.ChangeState(PlayerStateMachine.StateId.Attack);
        }
        else
        {
            context.PlayerController.MovePosition(context.PlayerController.SelectedEnemy.transform.position);
        }
    }

    public void SetChaseState(float chaseRange, PlayerStateMachine.StateId nextState)
    {
        range = chaseRange;
        this.nextState = nextState;
    }
}
