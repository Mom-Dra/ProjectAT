using PlayerStateMachine;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어와 상호작용할 수 있는 모든 오브젝트(문, 시체, 상자, 컴퓨터 등)가 반드시 구현해야 하는 규약
/// </summary>
public interface IInteractable
{
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
    /// 상호작용 도중 플레이어가 상호작용을 취소할 수 있는지 여부. (예: 문 열기 중에는 취소 불가능, 해킹 중에는 취소 가능)
    /// </summary>
    bool CanStopInteract{get; } 

    /// <summary>
    /// 상호작용이 끝난 후 전환할 다음 상태를 지목합니다.
    /// </summary>
    PlayerStateType NextState { get; }

    /// <summary>
    /// 상호작용이 시작될때 해당 오브젝트가 해야할 로직들을 정의합니다. (예: 다른 플레이어가 이미 상호작용 중인 경우 "사용 중" UI 띄우기, 상호작용 시작 사운드 재생 등)
    /// InteractingState의 OnEnter()에서 호출됩니다.
    /// </summary>
    /// <param name="player"></param>
    void OnInteractStart(PlayerController player);

    /// <summary>
    /// InteractDuration 시간이 다 찼을 때 실행됩니다.
    /// 이 안에서 실제로 문이 열리거나, 시체가 플레이어의 손에 붙는 처리를 합니다.
    /// </summary>
    void OnExecute(PlayerController player); //이 매개변수가 필요할지 고려하기.

    /// <summary>
    /// 성공여부에 상관 없이 상호작용이 모두 끝날 때 실행되는 메서드입니다. (예: 상호작용 애니메이션 끝나고 원래대로 돌아오기, UI 초기화 등)
    /// InteractingState의 OnExit()에서 호출됩니다.
    /// </summary>
    /// <param name="player"></param>
    void OnInteractEnd(PlayerController player); 

    /// <summary>
    /// 상호작용을 위해 플레이어가 도착해야 할 정확한 월드 좌표 및 Look Direction의 반환을 시도합니다. 
    /// 상호작용 위치가 특별히 정해져 있으면 그 오브젝트는 해당 위치의 Vector를 반환해야합니다. (예: 문 앞, 컴퓨터 앞 등)
    /// </summary>
    bool TryGetInteractLocation(Transform playerTransform, out Vector3 sampledPosition, out Vector3 sampledLookDir, NavMeshAgent agent);

    /// <summary>
    /// 상호작용을 시도하려 할 때 이미 다른 오브젝트와 상호작용 중인지 판단하고, 상호작용 중이지 않으면 해당 오브젝트를 등록한 후 다른 오브젝트와 상호작용 못하도록 잠그는 메서드
    /// </summary>
    /// <param name="interactor"></param>
    /// <returns></returns>
    bool TryLock(PlayerController interactor);

    /// <summary>
    /// 상호작용이 끝났거나 취소될 때 다른 오브젝트가 이 오브젝트와 상호작용할 수 있도록 잠금을 해제하는 메서드
    /// </summary>
    void UnLock(); // 상호작용이 끝났거나 취소될 때
}
