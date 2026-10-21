using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Main Enemy AI controller using Finite State Machine
/// FSM erabiltzen duen etsaiaren AI kontrol nagusia
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : NetworkBehaviour
{
    [Header("References")]
    //[SerializeField] private Transform playerTransform;

    [Header("Components")]
    public NavMeshAgent Agent { get; private set; }
    [SerializeField] public RangeDetector_FSM_Tutorial RangeDetector;
    [SerializeField] public LineOfSightDetector_FSM_Tutorial LineOfSightDetector;
    private Animator animator;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 4f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackDamage = 10f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // State Machine
    private EnemyStateMachine stateMachine;

    // States
    public IdleState IdleState { get; private set; }
    public PatrolState PatrolState { get; private set; }
    public ChaseState ChaseState { get; private set; }
    public AttackState AttackState { get; private set; }

    // Public properties
    public GameObject PlayerTarget { get; set; }
    public Vector3 LastKnownPlayerPosition { get; set; }
    public Transform[] PatrolPoints => patrolPoints;
    public float PatrolSpeed => patrolSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float AttackRange => attackRange;
    public float AttackDamage => attackDamage;

    private void Awake()
    {
        // Get components
        Agent = GetComponent<NavMeshAgent>();
        //RangeDetector = GetComponent<RangeDetector>();
        //LineOfSightDetector = GetComponent<LineOfSightDetector>();
        animator = GetComponentInChildren<Animator>(); // Animator can be in child object

        // Initialize state machine
        stateMachine = new EnemyStateMachine();

        // Create states
        IdleState = new IdleState(this, stateMachine);
        PatrolState = new PatrolState(this, stateMachine);
        ChaseState = new ChaseState(this, stateMachine);
        AttackState = new AttackState(this, stateMachine);
    }

    private void Start()
    {
        // Start with Idle state
        stateMachine.Initialize(IdleState);
        LastKnownPlayerPosition = transform.position;
    }

    private void Update()
    {
        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    /// <summary>
    /// Set animation state parameter
    /// Animazio egoera parametroa ezarri
    /// </summary>
    /// <param name="stateIndex">0=Idle, 1=Patrol, 2=Chase, 3=Attack</param>
    public void SetAnimationState(int stateIndex)
    {
        if (animator != null)
        {
            animator.SetInteger("CurrentState", stateIndex);
        }
    }

    /// <summary>
    /// Called by animation event when attack hits
    /// Animazio event-ak deituta erasoak jo duenean
    /// </summary>
    public void OnAttackHit()
    {
        if (PlayerTarget != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, PlayerTarget.transform.position);

            if (distanceToPlayer <= AttackRange)
            {
                Debug.Log($"HIT! Dealing {AttackDamage} damage to player!");

                // Apply damage to player (uncomment when PlayerHealth exists)
                // PlayerHealth playerHealth = PlayerTarget.GetComponent<PlayerHealth>();
                // if (playerHealth != null)
                // {
                //     playerHealth.TakeDamage(AttackDamage);
                // }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Draw patrol points
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);

                // Draw line to next patrol point
                int nextIndex = (i + 1) % patrolPoints.Length;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }
        }

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw last known position
        if (Application.isPlaying && PlayerTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(LastKnownPlayerPosition, 0.3f);
        }
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        // Draw current state info in Scene view
        if (Application.isPlaying && stateMachine != null && stateMachine.CurrentState != null)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3f,
                $"State: {stateMachine.CurrentState.GetType().Name}"
            );
        }
#endif
    }
}
