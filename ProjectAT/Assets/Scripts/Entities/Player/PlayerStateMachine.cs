using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerStateMachine : NetworkBehaviour
{
    public enum StateId : ushort { Idle, Walk, Run, Attack }

    [SerializeField] private PlayerController myController;
    [SerializeField] public NavMeshAgent MyAgent { get { return myController.MyAgent; } }
    [SerializeField] public EntityStatus MyStatus { get { return myController.MyStatus; } }
    [SerializeField] public EntityStatus MyWeaponStatus { get { return myController.MyStatus; } }
    public string myName;

    public EntityState CurrentState { get; private set; }
    public PlayerIdleStateBase IdleState { get; private set; }
    public PlayerWalkStateBase WalkState { get; private set; }
    public PlayerRunStateBase RunState { get; private set; }
    public PlayerAttackStateBase AttackState { get; private set; }

    public readonly Dictionary<StateId, EntityState> stateDic = new Dictionary<StateId, EntityState>();

    public override void OnNetworkSpawn()
    {
        myController = GetComponent<PlayerController>();
        StateInitialize();
        if (IsOwner)
        {
            myController.InitiateSettings();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            myController.UnLinkInputEventsAll();
        }
    }

    public void StateInitialize()
    {
        if (IsServer)
        {
            CurrentState = IdleState = new ServerPlayerIdleState(this);
            WalkState = new ServerPlayerWalkState(this);
            RunState = new ServerPlayerRunState(this);
            AttackState = new ServerPlayerAttackState(this);
        }
        else
        {
            CurrentState = IdleState = new PlayerIdleStateBase(this);
            WalkState = new PlayerWalkStateBase(this);
            RunState = new PlayerRunStateBase(this);
            AttackState = new PlayerAttackStateBase(this);
        }
        myName = Random.Range(1, 100).ToString();

        StatesAdd();
    }

    private void StatesAdd()
    {
        stateDic.Add(StateId.Idle, IdleState);
        stateDic.Add(StateId.Walk, WalkState);
        stateDic.Add(StateId.Run, RunState);
        stateDic.Add(StateId.Attack, AttackState);
    }

    [Rpc(SendTo.Server)]
    public void ChangeStateServerRpc(StateId nextState)
    {
        ChangeStateClientRpc(nextState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ChangeStateClientRpc(StateId nextState)
    {
        ChangeState(nextState);
    }

    private void ChangeState(StateId nextState)
    {
        CurrentState.Exit();
        CurrentState = stateDic[nextState];
        CurrentState.Enter();
    }

    public void HandleClickInput(PlayerInputType type)
    {
        if (!IsOwner) return;
        CurrentState.HandleClickInput(type);
    }

    public void FixedUpdate()
    {
        CurrentState.OnUpdate();
    }

    public bool IsArrivedToDest()
    {
        return MyAgent.destination == MyAgent.transform.position;
    }

    public bool IsClickSamePosition()
    {
        return myController.IsClickSamePosition();
    }

    public void PlayerMove()
    {
        if (IsOwner)
        {
            PlayerMoveServerRpc(GetMouseWorldPosition());
        }
    }

    [Rpc(SendTo.Server)]
    public void PlayerMoveServerRpc(Vector3 nextPos)
    {
        if (nextPos == Vector3.zero || nextPos == null)
        {
            Debug.Log($"{myName} : Vector is Zero. Returned.");
            return;
        }

        myController.MovePosition(nextPos);
        PlayerMoveClientRpc(nextPos);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void PlayerMoveClientRpc(Vector3 nextPos)
    {
        //애니메이션 및 이런거 저런거 추가
        //Debug.Log($"{NetworkBehaviourId} : 움직임 애니메이션 출력");
        //Debug.Log($"{NetworkBehaviourId} : 움직임 사운드 출력");
        myController.MovePosition(nextPos);
    }

    public EntityController FindNearEnemy()
    {
        return myController.FindNearEnemy();
    }

    public void AttackEnemy(EntityController target)
    {
        if (IsServer && target)
        {
            myController.RotateTo(target.transform);
            if (MyStatus.CanFire())
            {
                myController.AttackEnemy(target);
                AttackEnemyClientRpc(target);
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AttackEnemyClientRpc(NetworkBehaviourReference target)
    {
        if (target.TryGet(out EntityController enemy))
        {
            Debug.Log($"{myName} : 사격 애니메이션 출력");
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        return myController.GetMouseWorldPosition();
    }
}