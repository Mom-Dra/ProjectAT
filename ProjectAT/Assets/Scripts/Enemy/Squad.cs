using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using UnityEngine.AI;
using UnityEditorInternal;

// public interface ISquadMember
// {
//     event Action<ISquadMember, IPerceivable, Vector3> onPlayerDetected;
//     event Action<ISquadMember, Vector3> onPlayerLosted;
//     event Action<ISquadMember, Vector3> onPlayerPositionUpdated;

//     void ReceiveSquadAlert(IPerceivable target, Vector3 lastKnownPosition);
//     void SetFormationDestination(Vector3 targetDestination, Vector3 lastKnownPosition);
// }



// public class Squad : MonoBehaviour, ISquadMember
// {
//     public event Action<ISquadMember, IPerceivable, Vector3> onPlayerDetected;
//     public event Action<ISquadMember, Vector3> onPlayerLosted;
//     public event Action<ISquadMember, Vector3> onPlayerPositionUpdated;

//     [SerializeField]
//     private List<GameObject> memberGameObjects;

//     [Header("Formation Settings")]
//     [SerializeField]
//     private float formationOffsetDistance = 3f;

//     [SerializeField]
//     private float formationSampleRadius = 2f;

//     [SerializeField]
//     private float maxDistanceOffset = 1f;

//     private bool isSquadAlerted = false;
//     private Vector3 squadLastKnownPosition;
//     private IPerceivable target;
//     private List<ISquadMember> squadMembers = new List<ISquadMember>();

//     private void Awake()
//     {
//         foreach (GameObject memberObject in memberGameObjects)
//         {
//             if (memberObject.TryGetComponent(out ISquadMember enemy))
//             {
//                 Add(enemy);
//             }
//         }
//     }

//     public void Add(ISquadMember squadMember)
//     {
//         if (squadMembers.Contains(squadMember))
//         {
// #if UNITY_EDITOR

//             Debug.LogError($"{squadMember} already exists");
// #endif
//             return;
//         }

//         squadMembers.Add(squadMember);
//         squadMember.onPlayerDetected += PlayerDetected;
//         squadMember.onPlayerPositionUpdated += PlayerPositionUpdated;
//     }

//     public void Remove(ISquadMember squadMember)
//     {
//         if (!squadMembers.Contains(squadMember))
//         {
// #if UNITY_EDITOR
//             Debug.LogError($"{squadMember} already deleted");
// #endif
//             return;
//         }

//         squadMembers.Remove(squadMember);
//         squadMember.onPlayerDetected -= PlayerDetected;
//         squadMember.onPlayerPositionUpdated -= PlayerPositionUpdated;
//     }

//     public void PlayerDetected(ISquadMember enemy, IPerceivable target, Vector3 lastKnownPosition)
//     {
//         isSquadAlerted = true;
//         squadLastKnownPosition = lastKnownPosition;
//         this.target = target;

//         CalculateFormation(target.Transform, lastKnownPosition);

//         foreach (ISquadMember member in squadMembers)
//         {
//             if (member != enemy)
//             {
//                 member.ReceiveSquadAlert(target, lastKnownPosition);
//             }
//         }

//         onPlayerDetected?.Invoke(enemy, target, lastKnownPosition);
//     }

//     public void PlayerLosted(ISquadMember enemy, Vector3 lastKnownPosition)
//     {
//         if (!isSquadAlerted) return;

//         foreach (ISquadMember member in squadMembers)
//         {
//             if (member != enemy)
//             {
//                 return;
//             }
//         }
//     }

//     private void PlayerPositionUpdated(ISquadMember enemy, Vector3 position)
//     {
//         // target�� �ٲ� ���ɼ��� ����!

//         //ColorDebug.GreenLog($"PlayerPositionUpdated: {position}");

//         squadLastKnownPosition = position;
//         CalculateFormation(target.Transform, position);
//     }

//     public void ReceiveSquadAlert(IPerceivable target, Vector3 lastKnownPosition)
//     {
//         foreach (var member in squadMembers)
//         {
//             member.ReceiveSquadAlert(target, lastKnownPosition);
//         }
//     }

//     public void SetFormationDestination(Vector3 targetDestination, Vector3 lastKnownPosition)
//     {
//         // ���� �����尡 �� �������� ��ġ�� ����? (��������)
//     }

//     private void CalculateFormation(Transform target, Vector3 lastKnownPosition)
//     {
//         Vector3 targetPosition = target.position;
//         float angleStep = 360f / squadMembers.Count;

//         for (int i = 0; i < squadMembers.Count; ++i)
//         {
//             ISquadMember member = squadMembers[i];

//             float angle = angleStep * i;

//             Vector3 offset = Quaternion.Euler(0f, UnityEngine.Random.Range(angle, angle + angleStep), 0f) * Vector3.forward * (formationOffsetDistance + UnityEngine.Random.Range(-maxDistanceOffset, maxDistanceOffset));
//             Vector3 idealPosition = targetPosition + offset;

//             NavMeshHit hit;
//             Vector3 finalPosition = targetPosition;

//             if (NavMesh.SamplePosition(idealPosition, out hit, formationSampleRadius, NavMesh.AllAreas))
//             {
//                 finalPosition = hit.position;
//             }
//             else
//             {
//                 if (NavMesh.SamplePosition(targetPosition, out hit, formationOffsetDistance, NavMesh.AllAreas))
//                 {
//                     finalPosition = hit.position;
//                 }
//             }

//             //ColorDebug.Log($"offset: {offset}", Color.red);
//             //ColorDebug.Log($"finalPosition: {finalPosition}", Color.red);
//             member.SetFormationDestination(finalPosition, lastKnownPosition);
//         }
//     }
// }
