using UnityEngine;

/// <summary>
/// Attack State - Enemy attacks the player
/// Attack egoera - Etsaia jokalaria erasotzen du
/// </summary>
public class AttackState : EnemyState
{
    private float attackCooldown = 1.5f; // Time between attacks
    private float lastAttackTime = 0f;

    public AttackState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        Debug.Log("Entering ATTACK state");

        if (enemy.Agent != null)
        {
            enemy.Agent.isStopped = true;
            enemy.Agent.velocity = Vector3.zero;
        }

        lastAttackTime = -attackCooldown; // Allow immediate first attack
    }

    public override void Update()
    {
        enemy.SwitchToCloserVisiblePlayer();

        if (enemy.PlayerTarget == null)
        {
            stateMachine.ChangeState(enemy.IdleState);
            return;
        }

        // Check if player is still in sight
        GameObject playerInSight = enemy.LineOfSightDetector?.PerformDetection(enemy.PlayerTarget);

        if (playerInSight == null)
        {
            // Lost sight of player, go back to chase
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // Check distance to player
        float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.PlayerTarget.transform.position);

        if (distanceToPlayer > enemy.AttackRange)
        {
            // Player moved away, chase again
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        // Look at player
        Vector3 directionToPlayer = enemy.PlayerTarget.transform.position - enemy.transform.position;
        directionToPlayer.y = 0; // Keep rotation on horizontal plane
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            enemy.transform.rotation = Quaternion.Slerp(enemy.transform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        // Perform attack
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private void PerformAttack()
    {
        Debug.Log("ATTACKING!");

        if (enemy.PlayerTarget.TryGetComponent(out PlayerHealth playerHealth))
            playerHealth.TakeDamage(enemy.AttackDamage);
        
        // Apply damage to player (if player has health component)
        // PlayerHealth playerHealth = enemy.PlayerTarget.GetComponent<PlayerHealth>();
        // if (playerHealth != null)
        // {
        //     playerHealth.TakeDamage(enemy.AttackDamage);
        // }
    }

    public override void Exit()
    {
        Debug.Log("Exiting ATTACK state");
    }
}
