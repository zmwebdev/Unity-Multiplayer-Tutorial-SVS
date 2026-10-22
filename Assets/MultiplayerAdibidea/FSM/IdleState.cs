using UnityEngine;

/// <summary>
/// Idle State - Enemy stands still and looks around
/// Idle egoera - Etsaia geldirik egoten da eta ingurura begiratzen
/// </summary>
public class IdleState : EnemyState
{
    private float idleTimer;
    private float idleDuration = 3f; // How long to stay idle

    public IdleState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering IDLE state");
        idleTimer = 0f;

        // Stop movement
        if (enemy.Agent != null)
        {
            enemy.Agent.isStopped = true;
            enemy.Agent.velocity = Vector3.zero;
        }

    }

    public override void Update()
    {
        idleTimer += Time.deltaTime;

        // Check if player is in detection range
        GameObject detectedPlayer = enemy.RangeDetector?.UpdateDetector();
        if (detectedPlayer != null)
        {
            // Check line of sight
            GameObject playerInSight = enemy.LineOfSightDetector?.PerformDetection(detectedPlayer);
            if (playerInSight != null)
            {
                enemy.PlayerTarget = playerInSight;
                stateMachine.ChangeState(enemy.ChaseState);
                return;
            }
        }

        // After idle duration, start patrolling
        if (idleTimer >= idleDuration)
        {
            stateMachine.ChangeState(enemy.PatrolState);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exiting IDLE state");
    }
}
