using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple Chase Behavior - Enemy jokalaria jarraitzen du
/// Ikasleak Chase portaera bakarka ulertzeko
///
/// Students learn what Chase behavior does in isolation
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SimpleChaseBehavior : NetworkBehaviour
{
    [Header("Chase Settings")]
    private Transform targetToChase; // Server-ean exekuzio garaian aukeratzen da (hurbilen dagoen jokalaria)
    [SerializeField] private float chaseSpeed = 4f; // Chase abiadura
    [SerializeField] private float stopDistance = 2f; // Noiz gelditu (attack-rako)
    [SerializeField] private float updatePathInterval = 0.2f; // Path eguneratu maiztasuna

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private NavMeshAgent agent;
    private float pathUpdateTimer = 0f;

    public override void OnNetworkSpawn()
    {
        agent = GetComponent<NavMeshAgent>();

        // AI-a server-ean bakarrik; bezeroek posizioa NetworkTransform bidez jasotzen dute
        if (!IsServer)
        {
            agent.enabled = false;
            enabled = false;
            return;
        }

        agent.speed = chaseSpeed;
        agent.stoppingDistance = stopDistance;
    }

    private Transform FindNearestPlayer()
    {
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
        // Path eguneratu maiztasun jakin batekin (optimizazio)
        pathUpdateTimer += Time.deltaTime;

        if (pathUpdateTimer >= updatePathInterval)
        {
            pathUpdateTimer = 0f;

            // Jokalari konektatuen artean hurbilena aukeratu (irteten badira ere lehen berreskuratzen da)
            targetToChase = FindNearestPlayer();
            if (targetToChase != null)
                agent.SetDestination(targetToChase.position);
        }

        if (targetToChase == null) return;

        // Egiaztatu distantzia
        float distanceToTarget = Vector3.Distance(transform.position, targetToChase.position);

        if (showDebugInfo && distanceToTarget <= stopDistance)
        {
            Debug.Log($"Close enough to target! Distance: {distanceToTarget:F2}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Marraztu target-era lerro bat
        if (targetToChase != null)
        {
            float distance = Vector3.Distance(transform.position, targetToChase.position);
            Gizmos.color = distance <= stopDistance ? Color.red : Color.yellow;
            Gizmos.DrawLine(transform.position + Vector3.up, targetToChase.position + Vector3.up);
        }

        // Stop distance esfera
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Enemy status
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    // Helper: Distantzia eskuratu (beste script-etarako)
    public float GetDistanceToTarget()
    {
        if (targetToChase == null) return float.MaxValue;
        return Vector3.Distance(transform.position, targetToChase.position);
    }

    // Helper: Target-era heldu garen?
    public bool HasReachedTarget()
    {
        return GetDistanceToTarget() <= stopDistance;
    }
}
