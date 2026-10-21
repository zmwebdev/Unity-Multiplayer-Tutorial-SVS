using UnityEngine;

/// <summary>
/// Chase State - Enemy chases the player
/// Chase egoera - Etsaia jokalaria jarraitzen du
/// </summary>
public class ChaseState : EnemyState
{
    private float loseTargetTime = 5f; // Time before losing target
    private float timeSinceLastSeen = 0f;

    public ChaseState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering CHASE state");
        timeSinceLastSeen = 0f;

        if (enemy.Agent != null)
        {
            enemy.Agent.isStopped = false;
            enemy.Agent.speed = enemy.ChaseSpeed;
        }

        // Set animation to Chase (running)
        enemy.SetAnimationState(2); // 2 = Chase
    }

    public override void Update()
    {
        if (enemy.PlayerTarget == null)
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // Check if player is still in sight
        GameObject playerInSight = enemy.LineOfSightDetector?.PerformDetection(enemy.PlayerTarget);

        if (playerInSight != null)
        {
            // Player is visible, reset timer and update position
            timeSinceLastSeen = 0f;
            enemy.LastKnownPlayerPosition = enemy.PlayerTarget.transform.position;

            // Move towards player
            if (enemy.Agent != null)
            {
                enemy.Agent.SetDestination(enemy.PlayerTarget.transform.position);
            }

            // Check if close enough to attack
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.transform.position);
            if (distanceToPlayer <= enemy.AttackRange)
            {
                stateMachine.ChangeState(enemy.AttackState);
                return;
            }
        }
        else
        {
            // Player not visible, move to last known position
            timeSinceLastSeen += Time.deltaTime;

            if (enemy.Agent != null)
            {
                enemy.Agent.SetDestination(enemy.LastKnownPlayerPosition);
            }

            // If we've reached last known position and still no sight
            if (Vector3.Distance(enemy.transform.position, enemy.LastKnownPlayerPosition) <= 2f)
            {
                if (timeSinceLastSeen >= loseTargetTime)
                {
                    enemy.PlayerTarget = null;
                    stateMachine.ChangeState(enemy.IdleState);
                }
            }
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting CHASE state");
    }
}
