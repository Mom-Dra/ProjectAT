using JetBrains.Annotations;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerStateMachine : NetworkBehaviour
{
    public enum StateId : ushort { Idle, Walk, Run, Attack, SkillTargeting, SkillCasting }

    [SerializeField] private PlayerController myController;
    [SerializeField] private PlayerSkillStrategyMap mySkillMap = new PlayerSkillStrategyMap();

    [SerializeField] public PlayerController MyController { get { return myController; } }
    [SerializeField] public NavMeshAgent MyAgent { get { return myController.MyAgent; } }
    [SerializeField] public EntityStatus MyStatus { get { return myController.MyStatus; } }
    [SerializeField] public EntityStatus MyWeaponStatus { get { return myController.MyStatus; } }
    [SerializeField] public Animator MyAnim { get { return myController.MyAnim; } }

    public string myName;

    public EntityState CurrentState { get; private set; }
    public PlayerIdleStateBase IdleState { get; private set; }
    public PlayerWalkStateBase WalkState { get; private set; }
    public PlayerRunStateBase RunState { get; private set; }
    public PlayerAttackStateBase AttackState { get; private set; }
    public SkillTargetingStateBase SkillTargetingState{ get; private set; }
    public SkillCastingStateBase SkillCastingState { get; private set; }


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
            SkillTargetingState = new ServerSkillTargetingState(this); //DesginatedFire 인스턴스 공유 필요 > 스크립터블로 빼야할듯
            SkillCastingState  = new ServerSkillCastingState(this);

        }
        else
        {
            CurrentState = IdleState = new PlayerIdleStateBase(this);
            WalkState = new PlayerWalkStateBase(this);
            RunState = new PlayerRunStateBase(this);
            AttackState = new PlayerAttackStateBase(this);
            SkillTargetingState = new SkillTargetingStateBase(this);
            SkillCastingState = new SkillCastingStateBase(this);
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
        stateDic.Add(StateId.SkillTargeting, SkillTargetingState);
        stateDic.Add(StateId.SkillCasting, SkillCastingState);
    }

    [Rpc(SendTo.Server)]
    public void ChangeStateServerRpc(StateId nextState)
    {
        ChangeStateClientRpc(nextState);
    }

    [Rpc(SendTo.Server)]
    public void ChangeStateServerRpc(StateId nextState, int nextSkillIndex)
    {
        if (nextState == StateId.SkillTargeting || nextState == StateId.SkillCasting)
        {
            Debug.Log("ChangeState");
            ChangeStateClientRpc(nextState, nextSkillIndex);
        }
        else Debug.LogWarning("Call Changing Skill States function but next State is not Skill State");
    }


    [Rpc(SendTo.ClientsAndHost)]
    public void ChangeStateClientRpc(StateId nextState)
    {
        ChangeState(nextState);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ChangeStateClientRpc(StateId nextState, int nextSkillIndex)
    {
        ChangeSkillStrategy(nextSkillIndex);
        ChangeState(nextState);
    }

    private void ChangeState(StateId nextState)
    {
        CurrentState.Exit();
        CurrentState = stateDic[nextState];
        CurrentState.Enter();
    }

    private void ChangeSkillStrategy(int nextSkillIndex)
    {
        SkillTargetingState.SetSkillStrategy(mySkillMap.GetSkillStrategy(nextSkillIndex));
        SkillCastingState.SetSkillStrategy(mySkillMap.GetSkillStrategy(nextSkillIndex));
    }

    public void HandleInput(PlayerInputType type)
    {
        if (!IsOwner) return;
        CurrentState.HandleInput(type);
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
        myController.MovePosition(nextPos);
    }

    public Enemy FindNearEnemy()
    {
        return myController.FindNearEnemy();
    }

    public void AttackEnemy(Enemy target, int damage)
    {
        if (IsServer && target)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            toTarget.y = 0; //보정

            if (Vector3.Angle(transform.forward, toTarget) > 10.0f)
            {
                myController.LookAtTarget(toTarget);
            }
            else
                myController.AttackEnemy(target, damage);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void AttackEnemyClientRpc(NetworkBehaviourReference target)
    {
        if (target.TryGet(out Enemy enemy))
        {
            myController.MyEffectModule.GenerateFiringEffect();
        }
    }

    public Vector3 GetMouseWorldPosition()
    {
        return myController.GetMouseWorldPosition();
    }

    public Enemy RaycastEnemy()
    {
        return myController.RaycastEnemy();
    }
}