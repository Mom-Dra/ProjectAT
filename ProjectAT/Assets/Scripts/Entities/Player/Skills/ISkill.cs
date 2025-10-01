using Unity.Netcode;
using UnityEngine;

public interface ISkill
{
    public string SkillName { get; }
    public Enemy Target { get; set; }
    //public Image skillIcon;

    public void OnTargetingEnter(PlayerStateMachine context); //스킬 조준 모드 진입시 호출
    public void OnTargetingUpdate(PlayerStateMachine context); //스킬 조준 모드에서 매 프레임 호출
    public void OnTargetingExit(PlayerStateMachine context); //스킬 조준 모드 종료시 호출
    public bool TryCommit(PlayerStateMachine context); //스킬 시전 시도, 성공시 true 반환
    public void OnCastingUpdate(PlayerStateMachine context); //매 프레임마다 해야할 함수?
    public void OnExecute(PlayerStateMachine context); //스킬 시전 시 로직처리용
    public void OnFinish(PlayerStateMachine context); //스킬 시전 종료 후 해야할일 있을 때 호출
}
