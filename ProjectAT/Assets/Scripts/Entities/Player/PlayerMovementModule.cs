using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;

public class PlayerMovementModule : MonoBehaviour
{
    [SerializeField] private NavMeshAgent myAgent;
    [SerializeField] private EntityStatus myStatus;

    private void Awake()
    {
        myAgent = GetComponent<NavMeshAgent>();
        myStatus = GetComponent<EntityStatus>();
    }

    public bool IsAgentMoving()
    {
        return !myAgent.isStopped 
            && (myAgent.pathPending || myAgent.velocity.sqrMagnitude > 0.05f);
    }

    public void PlayerWalk(Vector3 newPos)
    {
        MovePosition(newPos, myStatus.WalkSpeed);
    }
    public void PlayerRun(Vector3 newPos)
    {
        MovePosition(newPos, myStatus.RunSpeed);
    }

    private void MovePosition(Vector3 newPos, float speed)
    {
        myAgent.isStopped = false;
        myAgent.speed = speed;
        myAgent.SetDestination(newPos);
    }

    public bool PlayerRotateToward(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;

        if ((direction - transform.forward).sqrMagnitude < 0.01f)
        {
            return true;
        }
        else
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector2.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            return false;
        }
    }

    public void PlayerMoveStop()
    {
        myAgent.isStopped = true;
        //myAgent.ResetPath();
        myAgent.velocity = Vector3.zero;
    }

}
