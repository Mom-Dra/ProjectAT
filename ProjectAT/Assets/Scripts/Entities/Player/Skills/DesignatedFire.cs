using UnityEngine;

public class DesignatedFire : ISkill
{
    public string SkillName => "Designated Fire";
    private PlayerController myController;

    public DesignatedFire(PlayerController myController)
    {
        this.myController = myController;
    }

    #region Used in TargetingState
    public bool SelectTarget()
    {
        RaycastHit hitted = myController.MouseRaycast();
        if (hitted.collider != null && hitted.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            myController.SetTargetEnemy(hitted.collider.gameObject.GetComponent<Enemy>());
            return true;
        }
        else
        {
            myController.SetTargetEnemy(null);
            return false;
        }
    }
    #endregion
    #region Used in CastingState
    public void OnSkillUpdate()
    {
        if (TryCommit() && myController.SmoothRotateToTarget(myController.SelectedEnemy.transform.position))
        {
            ActivateSkill();
        }
        else
        {
            myController.PlayerWalk(myController.SelectedEnemy.transform.position);
        }
    }

    public bool TryCommit()
    {
        return myController.IsInAttackRange(myController.SelectedEnemy);
    }

    public void ActivateSkill()
    {
        Debug.Log("Designated Fire Activated"); //TODO : Skill상태들 가서 Input처리할 차례
        myController.StopMoving();
        //myController.PlayDesignateAnimation();
        myController.DesignateFireToEnemy();
        myController.ChangeState(PlayerStateMachine.StateId.Idle);
    }

    public void OnFinish()
    {
        myController.SetTargetEnemy(null);
    }
    #endregion
}
