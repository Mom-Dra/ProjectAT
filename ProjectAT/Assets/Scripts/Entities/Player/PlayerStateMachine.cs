using JetBrains.Annotations;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerStateMachine
{
    public enum StateId : ushort { None, Idle, Walk, Run, Chase, Attack, SkillTargeting, SkillCasting }

    [SerializeField] private PlayerController myController;

    //컨트롤러
    public PlayerController PlayerController => myController;

    //states
    public EntityState CurrentState { get; private set; }
    public PlayerIdleStateBase IdleState { get; private set; }
    public PlayerWalkStateBase WalkState { get; private set; }
    public PlayerRunStateBase RunState { get; private set; }
    public PlayerChaseStateBase ChaseState { get; private set; }
    public PlayerAttackStateBase AttackState { get; private set; }
    public SkillTargetingStateBase SkillTargetingState{ get; private set; }
    public SkillCastingStateBase SkillCastingState { get; private set; }

    public readonly Dictionary<StateId, EntityState> stateDic = new Dictionary<StateId, EntityState>();

    public PlayerStateMachine(PlayerController controller)
    {
        myController = controller;
        Initialize();
    }

    #region 초기화
    private void Initialize()
    {
        StateInitialize();
        StatesAdd();
    }
    public void StateInitialize()
    {
        CurrentState = IdleState = new PlayerIdleStateBase(this);
        WalkState = new PlayerWalkStateBase(this);
        RunState = new PlayerRunStateBase(this);
        ChaseState = new PlayerChaseStateBase(this);
        AttackState = new PlayerAttackStateBase(this);
        SkillTargetingState = new SkillTargetingStateBase(this, myController.MySkillMap);
        SkillCastingState = new SkillCastingStateBase(this, myController.MySkillMap);
    }

    private void StatesAdd()
    {
        stateDic.Add(StateId.Idle, IdleState);
        stateDic.Add(StateId.Walk, WalkState);
        stateDic.Add(StateId.Run, RunState);
        stateDic.Add(StateId.Chase, ChaseState);
        stateDic.Add(StateId.Attack, AttackState);
        stateDic.Add(StateId.SkillTargeting, SkillTargetingState);
        stateDic.Add(StateId.SkillCasting, SkillCastingState);
    }

    #endregion
    #region 상태 제어 함수
    public void ChangeState(StateId nextState)
    {
        if (nextState == StateId.None)
        {
            Debug.Log("State None");
            return;
        }

        CurrentState?.Exit();
        CurrentState = stateDic[nextState];
        CurrentState.Enter();
    }

    public void HandleInput(PlayerInputType type)
    {
        CurrentState.HandleInput(type);
    }

    public void OnUpdate()
    {
        CurrentState.OnUpdate();
    }
    #endregion

}