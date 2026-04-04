using UnityEngine;

public static class PhysicsMathUtility
{
    /// <summary>
    /// 시작점에서 목표점까지 지정된 최고 높이(arcHeight)를 거쳐 날아가는 포물선 궤적 데이터를 계산합니다.
    /// </summary>
    /// <param name="origin">던지는 시작 위치</param>
    /// <param name="target">목표 바닥 위치</param>
    /// <param name="arcHeight">포물선의 최고 높이 (상대값)</param>
    /// <param name="initialVelocity">계산된 초기 속도 (결과값)</param>
    /// <param name="totalTime">목표까지 도달하는 데 걸리는 총 체공 시간 (결과값)</param>
    /// <returns>계산 성공 여부 (타겟이 포물선 최고점보다 높으면 false 반환)</returns>
    public static bool CalculateTrajectory(Vector3 origin, Vector3 target, float arcHeight, out Vector3 initialVelocity, out float totalTime)
    {
        // out 매개변수 초기화
        initialVelocity = Vector3.zero;
        totalTime = 0f;

        float gravity = Physics.gravity.y; // 기본적으로 음수 (-9.81)
        float displacementY = target.y - origin.y;

        // 예외 처리: 타겟이 설정한 포물선의 최고점보다 높으면 도달할 수 없습니다. (NaN 에러 방지)
        if (displacementY > arcHeight)
        {
            return false;
        }

        Vector3 displacementXZ = new Vector3(target.x - origin.x, 0, target.z - origin.z);

        // 최고점까지 올라가는 시간
        float timeUp = Mathf.Sqrt(-2 * arcHeight / gravity);
        // 최고점에서 타겟까지 떨어지는 시간
        float timeDown = Mathf.Sqrt(2 * (displacementY - arcHeight) / gravity);
        
        // 결과 1: 총 체공 시간
        totalTime = timeUp + timeDown;

        // Y축(수직) 초기 속도
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * arcHeight);
        // XZ축(수평) 속도 (수평은 등속 운동)
        Vector3 velocityXZ = displacementXZ / totalTime;

        // 결과 2: 최종 초기 속도 벡터
        initialVelocity = velocityXZ + velocityY;
        
        return true;
    }
}