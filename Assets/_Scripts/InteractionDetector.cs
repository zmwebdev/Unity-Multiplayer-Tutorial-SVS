using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [SerializeField] private float m_detectionRadius = 3f;
    [SerializeField] private float m_detectionAngle = 60f;
    [SerializeField] private LayerMask m_pickupLayer;

    private IInteractable m_closestInteractable;
    public IInteractable ClosestInteractable => m_closestInteractable;    
    private bool m_isOwner = false;

    public void Initialize(bool isOwner)
    {
        m_isOwner = isOwner;
    }

    private void Update()
    {
        if (m_isOwner == false)
        {
            return;
        }
        DetectInteractables();
    }
    
    private void DetectInteractables()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, m_detectionRadius, m_pickupLayer);

        float closestDistance = float.MaxValue;
        IInteractable candidate = null;

        foreach (Collider hit in hits)
        {
            IInteractable pickable = hit.GetComponent<IInteractable>();
            if (pickable == null)
                continue;

            Vector3 directionToPickable
                = (hit.transform.position - transform.position).normalized;
            float angleToPickable = Vector3.Angle(transform.forward, directionToPickable);

            if (angleToPickable > m_detectionAngle * 0.5f)
                continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                candidate = pickable;
            }
        }

        if (candidate != m_closestInteractable)
        {
            if (m_closestInteractable != null)
                m_closestInteractable.ToggleSelection(false);
            m_closestInteractable = candidate;
            if (m_closestInteractable != null)
                m_closestInteractable.ToggleSelection(true);

        }
    }

        
}
