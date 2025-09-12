using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class DesignatedFire : ISkill
{
    public string SkillName => "Designated Fire";
    public Enemy Target { get; set; } //EntityController와 Enemy 연동이 필요할듯.

    //public void OnAimEnter(PlayerStateMachine context){}
    //public void OnAimExit(PlayerStateMachine context){}

    //서버에서 사용하는 함수들
    public void OnUpdate(PlayerStateMachine context)
    {
        if(Target) 
        {
            if(context.RaycastEnemy() == Target)
            {
                Vector3 to = Target.transform.position - context.transform.position;
                if (to.magnitude <= context.MyAgent.stoppingDistance + 0.01f)
                {
                    OnExecute(context);
                    return;
                }
                else
                {
                   context.MyController.LookAtTarget(to);
                }
            }
            else
            {
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
        return false;
    }
}
