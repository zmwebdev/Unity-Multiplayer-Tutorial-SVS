using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Simple Patrol Behavior - Enemy patrol puntuen artean mugitzen da
/// Ikasleak Patrol portaera bakarka ulertzeko
///
/// Students learn what Patrol behavior does in isolation
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class SimplePatrolBehavior : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] patrolPoints; // Patrol puntuak
    [SerializeField] private float patrolSpeed = 2f; // Patrol abiadura
    [SerializeField] private float waypointReachedDistance = 1.5f; // Noiz kontsideratu "heldu"
    [SerializeField] private float waitTimeAtWaypoint = 1f; // Denbora waypoint-ean

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points assigned! Add patrol points in Inspector.");
            enabled = false;
            return;
        }

        if (showDebugInfo)
        {
            Debug.Log($"SimplePatrolBehavior started - {patrolPoints.Length} patrol points");
        }

        // Hasi lehenengo patrol point-era
        MoveToNextPatrolPoint();
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Itxaron waypoint-ean
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;

            if (waitTimer >= waitTimeAtWaypoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextPatrolPoint();
            }
            return;
        }

        // Egiaztatu waypoint-era heldu garen
        float distanceToWaypoint = Vector3.Distance(transform.position, patrolPoints[currentPatrolIndex].position);

        if (distanceToWaypoint <= waypointReachedDistance)
        {
            if (showDebugInfo)
            {
                Debug.Log($"Reached waypoint {currentPatrolIndex}. Waiting {waitTimeAtWaypoint}s...");
            }

            // Hasi itxaron
            isWaiting = true;
            agent.isStopped = true;
        }
    }

    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // Hurrengo patrol point-era joan
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (showDebugInfo)
        {
            Debug.Log($"Moving to patrol point {currentPatrolIndex}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // Marraztu patrol puntuak
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null) continue;

                // Waypoint sfera
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.5f);

                // Zenbakia
                if (Application.isPlaying)
                {
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(patrolPoints[i].position + Vector3.up * 2f, $"WP {i}");
#endif
                }

                // Lerro bat hurrengo waypoint-era
                int nextIndex = (i + 1) % patrolPoints.Length;
                if (patrolPoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
                }
            }

            // Marraztu uneko helburua
            if (Application.isPlaying && currentPatrolIndex < patrolPoints.Length)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, patrolPoints[currentPatrolIndex].position);
            }
        }

        // Enemy status
        Gizmos.color = isWaiting ? Color.yellow : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
