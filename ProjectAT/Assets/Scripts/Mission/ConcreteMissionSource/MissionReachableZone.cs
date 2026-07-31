using UnityEngine;
using ProjectAT.Mission;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class MissionReachableZone : MissionProgressSource
{
    [SerializeField] private DecalProjector decalProjector; // 영역을 표시하는 데칼 프로젝터. 원형을 강제함.
    [SerializeField] private SphereCollider areaCollider;
    public override MissionObjectiveType ObjectiveType => MissionObjectiveType.AreaReached;

    private void Awake()
    {
        decalProjector = GetComponentInChildren<DecalProjector>();
        areaCollider = GetComponent<SphereCollider>();
        InitArea();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ReportProgress();
        }
    }

    private void InitArea()
    {
        decalProjector.size = new Vector3(areaCollider.radius * 2, decalProjector.size.y, areaCollider.radius * 2);
    }
}
