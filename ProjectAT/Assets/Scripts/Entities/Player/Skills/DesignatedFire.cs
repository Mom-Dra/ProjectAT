using UnityEngine;

public class DesignatedFire : ISkill
{
    public string SkillName => "Designated Fire";
    public Enemy Target { get; set; } //EntityController와 Enemy 연동이 필요할듯.

    //public void OnAimEnter(PlayerStateMachine context){}
    //public void OnAimExit(PlayerStateMachine context){}

    //서버에서 사용하는 함수들
    public void OnUpdate(PlayerStateMachine context)
    {

    }

    public void OnExecute(PlayerStateMachine context)
    {
        context.AttackEnemy(Target.GetComponent<EntityController>());
    }

    public void OnFinish(PlayerStateMachine context)
    {

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
