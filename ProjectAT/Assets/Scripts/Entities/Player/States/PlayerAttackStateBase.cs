using UnityEditor;
using UnityEngine;

public class PlayerAttackStateBase : EntityState
{
    public PlayerAttackStateBase(PlayerStateMachine cxt) : base(cxt){ }

    public override void Enter()
    {
        context.PlayerController.MyAnim.SetBool("isFiring", true);
    }

    public override void Exit()
    {
        context.PlayerController.MyAnim.SetBool("isFiring", false);
    }

    public override void HandleInput(PlayerInputType type)
    {
        switch (type)
        {
            case PlayerInputType.RightClick:
                PlayerStateMachine.StateId nextState = context.PlayerController.CalCulateNextStateByMouseRaycast();
                if (nextState != PlayerStateMachine.StateId.Chase) //공격이 아니면
                {
                    context.PlayerController.SetTargetEnemy(null); //타겟 초기화
                    context.ChangeState(nextState);
                }
                break;
        }
    }

    public override void OnUpdate()
    {
        if(context.PlayerController.SelectedEnemy == null)  //적이 죽으면
        {
            context.ChangeState(PlayerStateMachine.StateId.Idle);
        }
        else if (!context.PlayerController.IsInAttackRange(context.PlayerController.SelectedEnemy)) //적이 사정거리 밖으로 나가면
        {
            context.ChaseState.SetChaseState(context.PlayerController.MyWeapon.Radius, PlayerStateMachine.StateId.Attack);
            context.ChangeState(PlayerStateMachine.StateId.Chase);
        }
        Debug.Log("공격!!");
        return;
    }
}
