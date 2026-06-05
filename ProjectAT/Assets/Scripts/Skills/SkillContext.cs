using UnityEngine;

public class SkillContext //struct가 아닌 class를 쓰는 이유는? 찾아보기 => 데이터가 많으므로 struct를 쓰면 항상 복사본이 만들어지기 때문.
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