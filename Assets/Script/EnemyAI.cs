using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack
    }
    


    public State currentState;

    [Header("References")]
    public Transform player;
    public Animator animator;
    public NavMeshAgent agent;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float idleTime = 3f;

    [Header("Detection")]
    
    [Range(0, 180)]
public float viewAngle = 60f;

    public float detectionRange = 10f;
    public float attackRange = 2f;

    private int patrolIndex;
    private float idleTimer;
    private bool isAttacking;
    private AttackType currentAttackType;
    [Header("Movement")]
public float patrolSpeed = 2f;
public float chaseSpeed = 5f;



    void Start()
{
    currentState = State.Idle;
    idleTimer = idleTime;

    // Auto find player by tag
    GameObject playerObj = GameObject.FindWithTag("Player");

    if (playerObj != null)
        player = playerObj.transform;
    else
        Debug.LogError("No GameObject with tag 'Player' found in scene!");
}


void Update()
{
    if (player == null) return;

    float distanceToPlayer = Vector3.Distance(transform.position, player.position);

    Vector3 dir = (player.position - transform.position).normalized;
    float angle = Vector3.Angle(transform.forward, dir);

    bool canSeePlayer = distanceToPlayer <= detectionRange && angle < viewAngle;

    // ===== STATE TRANSITIONS =====

    switch (currentState)
    {
        case State.Idle:
            if (canSeePlayer)
                currentState = State.Chase;
            break;

        case State.Patrol:
            if (canSeePlayer)
                currentState = State.Chase;
            break;

        case State.Chase:
            if (!canSeePlayer)
                currentState = State.Patrol;
            else if (distanceToPlayer <= attackRange)
                currentState = State.Attack;
            break;

        case State.Attack:
            if (!canSeePlayer)
                currentState = State.Patrol;
            else if (distanceToPlayer > attackRange)
                currentState = State.Chase;
            break;
    }

    // ===== EXECUTION =====

    switch (currentState)
    {
        case State.Idle:
            HandleIdle();
            break;

        case State.Patrol:
            HandlePatrol();
            break;

        case State.Chase:
            HandleChase(distanceToPlayer);
            break;

        case State.Attack:
            HandleAttack();
            break;
    }

    UpdateAnimator();
}



    void HandleIdle()
    {
        agent.isStopped = true;
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            GoToNextPatrolPoint();
        }
    }

    void HandlePatrol()
{
    agent.speed = patrolSpeed;
    agent.isStopped = false;

    if (!agent.pathPending && agent.remainingDistance < 0.5f)
    {
        currentState = State.Idle;
        idleTimer = idleTime;
    }
}


    void HandleChase(float distance)
{
    Debug.Log("Distance to player: " + distance);

    agent.speed = chaseSpeed;
    agent.isStopped = false;
    agent.SetDestination(player.position);

    if (distance <= attackRange)
    {
        currentState = State.Attack;
    }
}


    public float attackCooldown = 1.2f;
private float nextAttackTime;
private int attackIndex = 0;

void HandleAttack()
{
    agent.isStopped = true;

    if (Time.time < nextAttackTime)
        return;

    nextAttackTime = Time.time + attackCooldown;

    if (attackIndex == 0)
    {
        currentAttackType = AttackType.RightHook;
        animator.SetTrigger("RightHook");
        attackIndex = 1;
    }
    else
    {
        currentAttackType = AttackType.Uppercut;
        animator.SetTrigger("Uppercut");
        attackIndex = 0;
    }
}




    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        currentState = State.Patrol;

        agent.destination = patrolPoints[patrolIndex].position;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    void UpdateAnimator()
    {
        float speed = agent.velocity.magnitude;

        if (currentState == State.Chase)
            animator.SetFloat("Speed", 1f);
        else if (currentState == State.Patrol)
            animator.SetFloat("Speed", 0.5f);
        else
            animator.SetFloat("Speed", 0f);
    }

    // Called via animation event
    public void EndAttack()
{
}



    // Called via animation event during punch frame
    public void DealDamage()
    {
        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            player.GetComponent<PlayerHealth>()
      .TakeDamage(10, currentAttackType, transform.position);


        }
    }
    void OnDrawGizmosSelected()
{
    Gizmos.color = Color.yellow;
    Gizmos.DrawWireSphere(transform.position, detectionRange);
}

}

