using PlayerStateMachine;
using UnityEngine;

/// <summary>
/// 플레이어와 상호작용할 수 있는 모든 오브젝트(문, 시체, 상자, 컴퓨터 등)가 반드시 구현해야 하는 규약
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 상호작용을 위해 플레이어가 도착해야 할 정확한 월드 좌표를 반환합니다.
    /// 플레이어의 현재 위치에 따라 앞/뒤 좌표가 달라질 수 있도록 Transform을 받습니다.
    /// 상호작용 위치가 특별히 정해져 있으면 그 오브젝트는 특정 Transform이나 위치를 반환해야할 것입니다.
    /// </summary>
    Vector3 GetInteractPosition(Transform playerTransform);

    /// <summary>
    /// 상호작용 지점에 도착한 후, 플레이어가 바라봐야 할 방향 벡터를 반환합니다.
    /// 상호작용 위치가 특별히 정해져 있으면 그 오브젝트는 특정 위치의 Transform의 방향을 반환해야할 것임.
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
    /// 현재 이 오브젝트와 상호작용 중인 오브젝트 참조. null이면 상호작용 가능 상태. 일단 Enemy와 Player 모두 해당 되니 나중에 Entity같은 클래스가 생기면 그걸로 대체하기.
    /// </summary>
    GameObject CurrentInteractor { get; }
    /// <summary>
    /// 해당 오브젝트와 상호작용중인 플레이어가 있는지 여부를 빠르게 판단하는 프로퍼티.
    /// </summary>
    bool IsInUse {get; }

    /// <summary>
    /// 상호작용이 끝난 후 물건을 드는 상태(CarryingState)로 전환할지 여부를 결정합니다.
    /// true: 시체, 상자 / false: 문, 컴퓨터
    /// </summary>
    PlayerStateType NextState { get; }


    /// <summary>
    /// InteractDuration 시간이 다 찼을 때 실행됩니다.
    /// 이 안에서 실제로 문이 열리거나, 시체가 플레이어의 손에 붙는 처리를 합니다.
    /// </summary>
    void OnExecute(PlayerController player); //이 매개변수가 필요할지 고려하기.
}
