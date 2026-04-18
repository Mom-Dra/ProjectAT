using UnityEngine;

/// <summary>
/// 플레이어와 상호작용할 수 있는 모든 오브젝트(문, 시체, 상자, 컴퓨터 등)가 반드시 구현해야 하는 규약
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 상호작용을 위해 플레이어가 도착해야 할 정확한 월드 좌표를 반환합니다.
    /// (플레이어의 현재 위치에 따라 앞/뒤 좌표가 달라질 수 있도록 Transform을 받습니다)
    /// </summary>
    Vector3 GetInteractPosition(Transform playerTransform);

    /// <summary>
    /// 상호작용 지점에 도착한 후, 플레이어가 바라봐야 할 방향 벡터를 반환합니다.
    /// </summary>
    Vector3 GetInteractLookDir(Transform playerTransform);

    /// <summary>
    /// 상호작용을 완료하는 데 필요한 시간(초)입니다. (예: 0.1초 즉발, 3.0초 해킹)
    /// </summary>
    float InteractDuration { get; }

    /// <summary>
    /// 상호작용 시작 시 플레이어의 Animator에 전달할 트리거 파라미터 이름입니다. (예: "OpenDoor", "Pickup")
    /// </summary>
    string PlayerAnimationTrigger { get; }

    /// <summary>
    /// 상호작용이 끝난 후 물건을 드는 상태(CarryingState)로 전환할지 여부를 결정합니다.
    /// true: 시체, 상자 / false: 문, 컴퓨터
    /// </summary>
    bool IsCarryable { get; }


    /// <summary>
    /// 애니메이션의 OnInteract 이벤트가 호출되거나, InteractDuration 시간이 다 찼을 때 실행됩니다.
    /// 이 안에서 실제로 문이 열리거나, 시체가 플레이어의 손에 붙는 처리를 합니다.
    /// </summary>
    void OnInteract(PlayerController player);
}
