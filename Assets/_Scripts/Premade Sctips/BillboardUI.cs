using UnityEngine;

/// <summary>
/// THis script makes the UI Document showing player tag / name always face the camera.
/// </summary>
public class BillboardUI : MonoBehaviour
{
    private Transform m_cameraTransform;

    private void Awake()
    {
        m_cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        Vector3 direction = transform.position - m_cameraTransform.position;
        direction.y = 0; // Keep the billboard upright
        if(direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
