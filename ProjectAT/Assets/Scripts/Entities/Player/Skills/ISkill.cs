using Unity.Netcode;
using UnityEngine;

public interface ISkill
{
    public string SkillName { get; }
    //public Image skillIcon;

    //On Casting in StateMachine.Update() -> OnUpdate -if (TryCommit())-> ActivateSkill -> OnFinish
    public bool SelectTarget(); //스킬 조준 모드에서 타겟 선택 시도, 성공시 true 반환
    //public void OnTargetingUpdate(PlayerStateMachine context); //스킬 조준 모드에서 매 프레임 호출
    //public void OnTargetingExit(PlayerStateMachine context); //스킬 조준 모드 종료시 호출
    public void OnSkillUpdate(); //스킬 시전 중 매 프레임마다 해야할 함수
    public bool TryCommit(); //스킬 시전 시도, 성공시 true 반환
    public void ActivateSkill(); //스킬 능력 적용 함수
    public void OnFinish(); //스킬 시전 종료 후 해야할일 있을 때 호출
}
