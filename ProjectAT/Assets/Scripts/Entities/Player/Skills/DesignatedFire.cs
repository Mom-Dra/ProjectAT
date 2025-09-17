using System.Collections;
using UnityEngine;

public class DesignatedFire : ISkill
{
    public string SkillName => "Designated Fire";
    public Enemy Target { get; set; } //EntityController와 Enemy 연동이 필요할듯.

    //서버에서 사용하는 함수들
    public void OnCastingUpdate(PlayerStateMachine context)
    {
        if(Target) 
        {
            if(context.FindNearEnemy() == Target)
            {
                OnExecute(context);
            }
            else
            {
                Debug.Log("Set Destination");
                context.MyAgent.SetDestination(Target.transform.position);
            }
        }
        else
        {
            OnFinish(context);
        }
    }

    public void OnExecute(PlayerStateMachine context)
    {
        Debug.Log("Designated Fire");
        //context.AttackEnemy(Target.GetComponent<Enemy>());
        context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle);
    }

    public void OnFinish(PlayerStateMachine context)
    {
        Debug.Log("Designated Fire Finish");
        context.MyAgent.ResetPath();
        Target = null;
        context.ChangeStateServerRpc(PlayerStateMachine.StateId.Idle);
    }

    public bool TryCommit(PlayerStateMachine context)
    {
        Enemy enemy = context.RaycastEnemy();
        if (enemy)
        {
            Target = enemy;
            return true;
        }
        Debug.Log("Raycast Failed");
        return false;
    }

    public void OnTargetingEnter(PlayerStateMachine context)
    {
        //조준UI 활성화
        throw new System.NotImplementedException();
    }

    public void OnTargetingUpdate(PlayerStateMachine context)
    {
        //마우스 바라보기
        Vector3 vec = context.GetMouseWorldPosition() - context.transform.position;
        context.MyController.LookAtTarget(vec);
    }

    public void OnTargetingExit(PlayerStateMachine context)
    {
        //조준UI 비활성화
        throw new System.NotImplementedException();
    }
}
