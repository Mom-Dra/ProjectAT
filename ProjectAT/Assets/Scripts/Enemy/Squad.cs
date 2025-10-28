using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.AI;

public interface ISquadMember
{
    event Action<ISquadMember, Transform, Vector3> onPlayerDetected;
    event Action<ISquadMember, Vector3> onPlayerLosted;
    void ReceiveSquadAlert(Transform target, Vector3 lastKnownPosition);
    void SetFormationDestination(Vector3 position);
    bool IsPlayerStillVisible { get; }
}

public class Squad : MonoBehaviour, ISquadMember
{
    [SerializeField]
    private List<GameObject> memberGameObjects;

    [Header("Formation Settings")]
    [SerializeField]
    private float formationOffsetDistance = 3f;

    [SerializeField]
    private float formationSampleRadius = 2f;

    [SerializeField]
    private float maxDistanceOffset = 1f;

    private Vector3 squadLastKnownPosition; // 분대 전체가 공유하는 LKP
    private bool isSquadAlerted = false;

    private List<ISquadMember> squadMembers = new List<ISquadMember>();

    public event Action<ISquadMember, Transform, Vector3> onPlayerDetected;
    public event Action<ISquadMember, Vector3> onPlayerLosted;

    public bool IsPlayerStillVisible
    {
        get
        {
            foreach(ISquadMember squadMember in squadMembers)
            {
                if (!squadMember.IsPlayerStillVisible) return false;
            }

            return true;
        }
    }

    private void Awake()
    {
        foreach (GameObject memberObject in memberGameObjects)
        {
            if(memberObject.TryGetComponent(out ISquadMember enemy))
            {
                Add(enemy);
            }
        }
    }

    public void Add(ISquadMember squadMember)
    {
#if UNITY_EDITOR
        if (squadMembers.Contains(squadMember))
            throw new ArgumentException($"{squadMember} already exists");
#endif

        squadMembers.Add(squadMember);
        squadMember.onPlayerDetected += PlayerDetected;
    }

    public void Remove(ISquadMember squadMember)
    {
#if UNITY_EDITOR
        if (!squadMembers.Contains(squadMember))
            throw new ArgumentException($"{squadMember} already deleted");
#endif

        squadMembers.Remove(squadMember);
        squadMember.onPlayerDetected -= PlayerDetected;
    }

    public void PlayerDetected(ISquadMember enemy, Transform target, Vector3 lastKnownPosition)
    {
        isSquadAlerted = true;
        squadLastKnownPosition = lastKnownPosition;

        CalculateFormation(target, lastKnownPosition);

        foreach (ISquadMember member in squadMembers)
        {
            if (member != enemy)
            {
                member.ReceiveSquadAlert(target, lastKnownPosition);
            }
        }

        onPlayerDetected?.Invoke(enemy, target, lastKnownPosition);
    }

    public void PlayerLosted(ISquadMember enemy, Vector3 lastKnownPosition)
    {
        if (!isSquadAlerted) return; // 이미 경계가 풀렸으면 무시

        // 잠시 후 (예: 1초 후) 분대 전체가 플레이어를 보고 있는지 확인
        // 코루틴을 쓰거나 간단히 딜레이를 줄 수 있음. 
        // 여기서는 즉시 확인하는 대신, "모든 분대원이 플레이어를 놓쳤을 때"를 확인하는 로직을 구현.

        // 다른 분대원 중 한 명이라도 여전히 플레이어를 보고 있는가?
        foreach (ISquadMember member in squadMembers)
        {
            if (member != enemy && member.IsPlayerStillVisible)
            {
                return; // 한 명이라도 보고 있으면 더 확인할 필요 없음
            }
        }

        // 만약 "아무도" 플레이어를 못 보고 있다면 = 분대 전체가 시야를 잃음

        // "분산 수색" 시작!
        //StartSquadSearch(lastKnownPosition);
    }

    public void ReceiveSquadAlert(Transform target, Vector3 lastKnownPosition)
    {
        foreach (var member in squadMembers)
        {
            member.ReceiveSquadAlert(target, lastKnownPosition);
        }
    }

    public void SetFormationDestination(Vector3 position)
    {
        // 상위 스쿼드가 이 스쿼드의 위치를 지정? (복잡해짐)
    }

    private void CalculateFormation(Transform target, Vector3 lastKnownPosition)
    {
        Vector3 targetPosition = target.position;
        float angleStep = 360f / squadMembers.Count;

        // Todo
        // 칼각으로 원형배치가 너무 부자연스러움 자연스럽게 가자
        for(int i = 0; i < squadMembers.Count; ++i)
        {
            ISquadMember member = squadMembers[i];

            float angle = angleStep * i;

            Vector3 offset = Quaternion.Euler(0f, UnityEngine.Random.Range(angle, angle + angleStep), 0f) * Vector3.forward * (formationOffsetDistance + UnityEngine.Random.Range(-maxDistanceOffset, maxDistanceOffset));
            Vector3 idealPosition = targetPosition + offset;

            NavMeshHit hit;
            Vector3 finalPosition = targetPosition;

            if (NavMesh.SamplePosition(idealPosition, out hit, formationSampleRadius, NavMesh.AllAreas))
            {
                finalPosition = hit.position;

                ColorDebug.Log($"ideal", Color.red);
            }
            else
            {
                if (NavMesh.SamplePosition(targetPosition, out hit, formationOffsetDistance, NavMesh.AllAreas))
                {
                    finalPosition = hit.position;

                    ColorDebug.Log($"recalculate", Color.red);
                }
            }

            ColorDebug.Log($"offset: {offset}", Color.red);
            ColorDebug.Log($"finalPosition: {finalPosition}", Color.red);
            member.SetFormationDestination(finalPosition);
        }
    }

    //private void StartSquadSearch(Vector3 lastKnownPosition)
    //{
    //    foreach(ISquadMember squadMember in squadMembers)
    //    {
    //        Vector3 searchPoint;

    //        // LKP 주변 랜덤 위치 (원형으로)
    //        Vector3 randomDir = UnityEngine.Random.insideUnitCircle * searchRadius;
    //        Vector3 searchPosCandidate = lastKnownPosition + new Vector3(randomDir.x, 0f, randomDir.y);

    //        NavMeshHit hit;
    //        // NavMesh 상의 유효한 위치인지 확인
    //        if (NavMesh.SamplePosition(searchPosCandidate, out hit, 5f, NavMesh.AllAreas))
    //        {
    //            searchPoint = hit.position;
    //        }
    //        else
    //        {
    //            searchPoint = lastKnownPosition; // 실패 시 그냥 LKP로 보냄
    //        }

    //        // 분대원에게 수색 지점 할당
    //        squadMember.SetSearchPoint(searchPoint);
    //    }
    //}
}
