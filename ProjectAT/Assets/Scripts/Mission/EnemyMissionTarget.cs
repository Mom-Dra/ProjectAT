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
            ReportProgress();
        }
    }
}