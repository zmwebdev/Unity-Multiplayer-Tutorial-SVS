using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Simple Attack Behavior - Enemy jokalaria erasotzen du
/// Ikasleak Attack portaera bakarka ulertzeko
///
/// Students learn what Attack behavior does in isolation
/// </summary>
public class SimpleAttackBehavior : NetworkBehaviour
{
    [Header("Attack Settings")]
    private Transform targetToAttack; // Server-ean exekuzio garaian aukeratzen da (hurbilen dagoen jokalaria)
    [SerializeField] private float attackRange = 2.5f; // Eraso distantzia
    [SerializeField] private float attackCooldown = 1.5f; // Erasoen arteko denbora
    [SerializeField] private float attackDamage = 10f; // Damage
    [SerializeField] private bool lookAtTarget = true; // Target-era begiratu?

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private float lastAttackTime = -999f; // Azken erasoaren denbora
    private bool isInRange = false;

    public override void OnNetworkSpawn()
    {
        // Eraso logika server-ean bakarrik
        if (!IsServer)
            enabled = false;
    }

    private Transform FindNearestPlayer()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || !manager.IsServer || !manager.IsListening)
            return null;

        Transform best = null;
        float minSqr = float.MaxValue;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject player = client.PlayerObject;
            if (player == null) continue;

            float sqr = (player.transform.position - transform.position).sqrMagnitude;
            if (sqr < minSqr)
            {
                minSqr = sqr;
                best = player.transform;
            }
        }

        return best;
    }

    private void Update()
    {
        targetToAttack = FindNearestPlayer();
        if (targetToAttack == null)
        {
            isInRange = false;
            return;
        }

        // Kalkulatu distantzia
        float distanceToTarget = Vector3.Distance(transform.position, targetToAttack.position);
        isInRange = distanceToTarget <= attackRange;

        // Target-era begiratu
        if (isInRange && lookAtTarget)
        {
            Vector3 direction = targetToAttack.position - transform.position;
            direction.y = 0; // Horizontal plane-an soilik
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        // Erasotzeko saiakera
        if (isInRange && CanAttack())
        {
            PerformAttack();
        }
    }

    private bool CanAttack()
    {
        // Cooldown-a pasa da?
        return Time.time >= lastAttackTime + attackCooldown;
    }

    private void PerformAttack()
    {
        lastAttackTime = Time.time;

        if (showDebugInfo)
        {
            Debug.Log($"⚔️ ATTACKING {targetToAttack.name}! Damage: {attackDamage}");
        }

        // Hemen animazioa trigger egin
        // animator?.SetTrigger("Attack");

        if (targetToAttack.TryGetComponent(out PlayerHealth playerHealth))
            playerHealth.TakeDamage(attackDamage);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Attack range esfera
        Gizmos.color = isInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Lerro bat target-era (range barruan baldin badago)
        if (targetToAttack != null && isInRange)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, targetToAttack.position + Vector3.up);
        }

        // Enemy status
        Gizmos.color = isInRange ? Color.red : Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Attack cooldown bar (visual)
        if (Application.isPlaying)
        {
            float cooldownProgress = Mathf.Clamp01((Time.time - lastAttackTime) / attackCooldown);
            Gizmos.color = cooldownProgress >= 1f ? Color.green : Color.red;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                $"Cooldown: {cooldownProgress * 100:F0}%"
            );
#endif
        }
    }
}
