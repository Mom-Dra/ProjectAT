using UnityEngine;

public class CoverHandler : MonoBehaviour
{
    private float maxCoverBonus = 0.5f;
    public Transform currentCover;

    public float GetCoverBonus(Transform attacker)
    {
        if (currentCover is null || attacker is null) return 0f;

        // 1. 엄폐물 방어 방향 (엄폐물의 Forward라고 가정)
        Vector3 coverNormal = currentCover.forward;

        // 2. 나(유닛)로부터 공격자를 향하는 방향
        Vector3 dirToAttacker = (attacker.position - transform.position).normalized;

        // 3. 내적 계산 (1이면 정면, 0이면 측면, -1이면 후면)
        float dot = Vector3.Dot(coverNormal, dirToAttacker);

        // 4. 보너스 판정 (정면 기준 약 60도 이내일 때만 보너스 적용)
        if (dot > 0.5f)
        {
            // 각도가 완벽할수록 보너스 증가 (0.5~1.0 사이를 0~1.0으로 매핑)
            float angleFactor = Mathf.Clamp01((dot - 0.5f) / 0.5f);
            return maxCoverBonus * angleFactor;
        }

        return 0f; // 측면이나 후면 공격은 보너스 없음
    }
}
