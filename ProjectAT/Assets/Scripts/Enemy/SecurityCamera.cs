using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class SecurityCamera : MonoBehaviour, ISquadMember
{
    public event Action<ISquadMember, Transform, Vector3> onPlayerDetected;
    public event Action<ISquadMember, Vector3> onPlayerLosted;
    public event Action<ISquadMember, Vector3> onPlayerPositionUpdated;

    [SerializeField]
    private SecurityCameraData securityCameraData;

    private List<Transform> playersInRange = new List<Transform>();
    private bool alarmTriggered = false;
    private float currTime;

    private Light cameraLight;

    private Coroutine rotateCoroutine;
    private Coroutine playerVisibleCoroutine;
    private Coroutine playerNonVisibleCoroutine;

    public bool IsPlayerStillVisible => throw new NotImplementedException();

    private void Awake()
    {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = securityCameraData.DetectionRange;

        cameraLight = GetComponentInChildren<Light>();
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(securityCameraData.ViewAngle, 0f, 0f);
        StartRotate();
    }

    private void OnTriggerEnter(Collider other)
    {
        ColorDebug.GreenLog("OnTriggerEnter");

        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ColorDebug.GreenLog("OnTriggerEnter Inner");

            playersInRange.Add(other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        ColorDebug.GreenLog("OnTriggerStay");

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ColorDebug.GreenLog("OnTriggerStay Inner");

            bool anyPlayerVisible = false;
            Transform player = default;

            foreach(Transform currPlayer in playersInRange)
            {
                if(CanSeePlayer(currPlayer))
                {
                    anyPlayerVisible = true;
                    player = currPlayer;
                    break;
                }
            }

            // 시야에 보이면
            // 비율 증가
            // 시야에 보이지 않으면 비율 감소

            if (anyPlayerVisible)
            {
                StopPlayerNonVisibleCoroutine();
                StartPlayerVisibleCoroutine(player);
            }
            else
            {
                StopPlayerVisibleCoroutine();
                StartPlayerNonVisibleCoroutine();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ColorDebug.GreenLog("OnTriggerExit");

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ColorDebug.GreenLog("OnTriggerExit Inner");

            playersInRange.Remove(other.transform);

            if (playersInRange.Count == 0)
            {
                StopPlayerVisibleCoroutine();
                StartPlayerNonVisibleCoroutine();
            }
        }
    }

    private void StartRotate()
    {
        if(rotateCoroutine is null)
        {
            rotateCoroutine = StartCoroutine(RotateCoroutine());
        }
    }

    private void StopRotate()
    {
        if (rotateCoroutine is not null)
        {
            StopCoroutine(rotateCoroutine);
        }
    }

    private IEnumerator RotateCoroutine()
    {
        while (true)
        {
            float pingPong = Mathf.PingPong(Time.time * securityCameraData.RotateSpeed, securityCameraData.RotateAngle * 2);
            float targetAngle = pingPong - securityCameraData.ViewAngle;

            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z);

            yield return null;
        }
    }

    private void StartPlayerVisibleCoroutine(Transform player)
    {
        if (playerVisibleCoroutine is null)
        {
            playerVisibleCoroutine = StartCoroutine(PlayerVisibleCoroutine(player));
        }
    }

    private void StopPlayerVisibleCoroutine()
    {
        if(playerVisibleCoroutine is not null)
        {
            StopCoroutine(playerVisibleCoroutine);
            playerVisibleCoroutine = null;
        }
    }

    private void StartPlayerNonVisibleCoroutine()
    {
        if (playerNonVisibleCoroutine is null)
        {
            playerNonVisibleCoroutine = StartCoroutine(PlayerNonVisibleCoroutine());
        }
    }

    private void StopPlayerNonVisibleCoroutine()
    {
        if (playerNonVisibleCoroutine is not null)
        {
            StopCoroutine(playerNonVisibleCoroutine);
            playerNonVisibleCoroutine = null;
        }
    }

    private IEnumerator PlayerVisibleCoroutine(Transform player)
    {
        while (currTime < securityCameraData.DetectionTime)
        {
            currTime += Time.deltaTime;
            UpdateDetectionVisual();

            yield return null;
        }

        currTime = securityCameraData.DetectionTime;
        UpdateDetectionVisual();

        ColorDebug.GreenLog("Camera: onPlayerDetected!!!");
        onPlayerDetected?.Invoke(this, player, player.position);
    }

    private IEnumerator PlayerNonVisibleCoroutine()
    {
        while(currTime > 0f)
        {
            currTime -= Time.deltaTime;
            UpdateDetectionVisual();

            yield return null;
        }

        currTime = 0f;
        UpdateDetectionVisual();
    }

    private void UpdateDetectionVisual()
    {
        float ratio = Mathf.Clamp01(currTime / securityCameraData.DetectionTime);

        UpdateVisual(ratio);
    }

    private void UpdateVisual(float ratio)
    {
        cameraLight.color = Color.Lerp(Color.white, Color.red, ratio);
    }

    private bool CanSeePlayer(Transform player)
    {
        // 거리
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer > securityCameraData.DetectionRange)
            return false;

        // 시야각
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > securityCameraData.DetectionAngle / 2f)
            return false;

        // 장애물
        RaycastHit hit;

        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer, securityCameraData.ObstacleMask))
            return false;

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        // 탐지 범위 원 그리기
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, securityCameraData.DetectionRange);

        // 시야각 그리기
        float halfFOV = securityCameraData.DetectionAngle / 2f;
        Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
        Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);

        Vector3 leftRayDirection = leftRayRotation * transform.forward;
        Vector3 rightRayDirection = rightRayRotation * transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, leftRayDirection * securityCameraData.DetectionRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * securityCameraData.DetectionRange);

        // 플레이어 탐지 시선 그리기
        foreach(Transform currPlayer in playersInRange)
        {
            if (CanSeePlayer(currPlayer))
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currPlayer.position);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currPlayer.position);
            }
        }
    }

    public void ReceiveSquadAlert(Transform target, Vector3 lastKnownPosition)
    {

    }

    public void SetFormationDestination(Vector3 targetDestination, Vector3 lastKnownPosition)
    {

    }
}
