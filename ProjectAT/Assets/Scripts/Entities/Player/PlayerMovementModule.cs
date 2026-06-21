using UnityEngine;
using UnityEngine.AI;

public class PlayerMovementModule : MonoBehaviour
{
    [SerializeField] private NavMeshAgent myAgent;
    [SerializeField] private EntityStatus myStatus;

    public float deltaRotation = 20f;

    public NavMeshAgent Agent => myAgent;

    private void Awake()
    {
        myAgent = GetComponent<NavMeshAgent>();
        myStatus = GetComponent<EntityStatus>();
    }

    private void Start()
    {
    }

    public bool IsAgentMoving()
    {
        return !myAgent.isStopped 
            && (myAgent.pathPending || myAgent.velocity.sqrMagnitude > 0.05f);
    }

    public bool IsAgentArrived()
    {
        return !myAgent.pathPending && myAgent.remainingDistance <= myAgent.stoppingDistance;
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
        if(newPos == Vector3.zero)
        {
            Debug.LogWarning("PlayerMovementModule: MovePosition() called with Vector3.zero. Check if the target position is valid.");
        }
        
        myAgent.updateRotation = true;
        myAgent.isStopped = false;
        myAgent.speed = speed;
        myAgent.SetDestination(newPos);
    }

    public bool PlayerRotateToward(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;
        
        if ((direction.normalized - transform.forward).sqrMagnitude < 0.0001f)
        {  
            myAgent.updateRotation = true;
            return true;
        }
        else
        {
            myAgent.updateRotation = false;
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * deltaRotation);
            return false;
        }
    }

    public void PlayerRotateImmediately(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = lookRotation;
        }
    }
    
    public void PlayerMoveStop()
    {
        if (myAgent.isStopped) return;
        myAgent.isStopped = true;
        //myAgent.ResetPath();
        myAgent.velocity = Vector3.zero;
    }

    public float GetVelocity()
    {
        return myAgent.velocity.magnitude;
    }

    public bool CanReachPosition(Vector3 targetPos)
    {
        NavMeshPath path = new NavMeshPath();
        if(!myAgent.CalculatePath(targetPos, path))
        {
            return false;
        }
        return path.status == NavMeshPathStatus.PathComplete;
    }
}
