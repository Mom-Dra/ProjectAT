using UnityEngine;

public class SkillContext
{
    //실행할 스킬
    public Skill SkillToExecute;

    //런타임에 유저가 지정한 타겟 (상태 머신이 추적할 대상)
    public GameObject TargetObject;
    public Vector3 CastedPosition;

    //무기 성능과 버프가 모두 덧셈/곱셈 완료된 최종 스탯
    public int FinalDamage;
    public float FinalRange;
}