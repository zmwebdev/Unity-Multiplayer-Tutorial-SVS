using UnityEngine;

/// <summary>
/// Patrol State - Enemy moves between patrol points
/// Patrol egoera - Etsaia patrol puntuen artean mugitzen da
/// </summary>
public class PatrolState : EnemyState
{
    private int currentPatrolIndex = 0;
    private float waypointReachedDistance = 1.5f;

    public PatrolState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering PATROL state");

        if (enemy.Agent != null)
        {
            enemy.Agent.isStopped = false;
            enemy.Agent.speed = enemy.PatrolSpeed;
        }

        // Go to first patrol point
        MoveToNextPatrolPoint();
    }

    public override void Update()
    {
        // Check if player is detected
        GameObject detectedPlayer = enemy.RangeDetector?.UpdateDetector();
        if (detectedPlayer != null)
        {
            GameObject playerInSight = enemy.LineOfSightDetector?.PerformDetection(detectedPlayer);
            if (playerInSight != null)
            {
                enemy.PlayerTarget = playerInSight;
                stateMachine.ChangeState(enemy.ChaseState);
                return;
            }
        }

        // Check if reached current waypoint
        if (enemy.PatrolPoints != null && enemy.PatrolPoints.Length > 0)
        {
            float distance = Vector3.Distance(enemy.transform.position, enemy.PatrolPoints[currentPatrolIndex].position);

            if (distance <= waypointReachedDistance)
            {
                // Move to next patrol point
                MoveToNextPatrolPoint();
            }
        }
        else
        {
            // No patrol points, go back to idle
            stateMachine.ChangeState(enemy.IdleState);
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (enemy.PatrolPoints == null || enemy.PatrolPoints.Length == 0)
            return;

        currentPatrolIndex = (currentPatrolIndex + 1) % enemy.PatrolPoints.Length;

        if (enemy.Agent != null)
        {
            enemy.Agent.SetDestination(enemy.PatrolPoints[currentPatrolIndex].position);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting PATROL state");
    }
}
