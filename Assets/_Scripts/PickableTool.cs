using Unity.Netcode.Components;
using UnityEngine;

public class PickableTool : PickableBase
{
    [SerializeField] private ComponentController m_componentController;
    protected override void ApplyAvailabilityState(bool newValue)
    {
        if (IsServer)
        {
            m_componentController.SetEnabled(newValue);
        }
    }

    protected override void OnPickedUp()
    {
        // no code needed here, the component controller will handle the state change on the server
    }

    public void Drop(Vector3 position)
    {
        // if not server return
        if (IsServer == false)
        {
            return;
        }

        transform.position = new Vector3(position.x, transform.position.y, position.z);
        m_isAvailable.Value = true;
    }
}
