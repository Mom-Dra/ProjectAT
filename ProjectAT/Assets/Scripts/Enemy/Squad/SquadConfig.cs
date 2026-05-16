using UnityEngine;

[CreateAssetMenu(fileName = "SquadConfig", menuName = "Scriptable Objects/SquadConfig")]
public class SquadConfig : ScriptableObject
{
    [Header("Formation")]
    [Tooltip("원형 포위 반경")]
    public float formationRadius = 5f;
    [Tooltip("대형 슬롯 재배치 주기(초)")]
    public float formationUpdateInterval = 0.5f;
    [Tooltip("이 거리(m) 이상 타겟이 이동했을 때만 대형을 재푸시. 0이면 항상 갱신.")]
    public float formationMinUpdateDistance = 1f;

    [Header("Search")]
    [Tooltip("탐색 반경(LastKnownPosition 기준)")] public float searchRadius = 8f;
    [Tooltip("탐색 상태 지속시간(초). 초과 시 Patrol 복귀")] public float searchDuration = 12f;
    [Tooltip("탐색 지점 개수")] public int searchPointCount = 6;
    [Tooltip("탐색 중 새 지점 재할당 주기")] public float searchReassignInterval = 2f;

    [Header("Movement Speeds")]
    public float patrolSpeed = 2.0f;
    public float engageSpeed = 4.5f;
    public float searchSpeed = 3.0f;

    [Header("Attack")]
    [Tooltip("EnemyData.AttackRange가 없거나 0일 때 사용할 기본값")]
    public float defaultAttackRange = 10f;
    [Tooltip("공격 쿨다운(초)")] public float fireCooldown = 0.5f;

    [Header("NavMesh Sampling")]
    [Tooltip("NavMesh 근처 샘플링 허용 반경")] public float navSampleRadius = 3f;

    [Header("Attack Mode Mapping")]
    [Tooltip("Search 상태에서도 detector의 AttackMode를 true로 유지할지 여부 " +
             "(true: 360° 긴 범위 / false: FOV 기반 탐색)")]
    public bool searchUsesAttackMode = false;
}
