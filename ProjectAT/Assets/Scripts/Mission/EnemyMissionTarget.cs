using UnityEngine;


namespace ProjectAT.Mission
{
    [RequireComponent(typeof(EntityStatus))]
    public class EnemyMissionTarget : MissionProgressSource
    {
        private EntityStatus entityStatus;
        public override MissionObjectiveType ObjectiveType => MissionObjectiveType.Kill;

        private void Awake()
        {
            entityStatus = GetComponent<EntityStatus>();
        }

        private void OnEnable()
        {
            if(entityStatus != null)
            {
                entityStatus.onDeath += HandleTargetDied;
            }
        }

        private void OnDisable()
        {
            if(entityStatus != null)
            {
                entityStatus.onDeath -= HandleTargetDied;
            }
        }

        private void HandleTargetDied()
        {
            ReportProgress(); //현재 사망 이벤트에는 공격자 정보가 필요 없으므로 actor는 null로 전달
        }
    }
}