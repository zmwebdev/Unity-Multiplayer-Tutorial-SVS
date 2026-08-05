using Unity.Netcode;
using UnityEngine;

public abstract class PickableBase : NetworkBehaviour, IInteractable
{
    // https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/networkvariable.html
    protected NetworkVariable<bool> m_isAvailable = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private SelectionOutline m_outline;
    [SerializeField] private ObjectType m_objectType;

    public bool canBePickedUp => m_isAvailable.Value;
    public ObjectType objectType => m_objectType;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_isAvailable.OnValueChanged += OnIsAvailableChanged;
        ApplyAvailabilityState(m_isAvailable.Value);
    }

    public override void OnNetworkDespawn()
    {
        m_isAvailable.OnValueChanged -= OnIsAvailableChanged;
        base.OnNetworkDespawn();
    }

    private void OnIsAvailableChanged(bool previousValue, bool newValue)
    {
       ApplyAvailabilityState(newValue);
    }

    protected abstract void ApplyAvailabilityState(bool newValue);

    public void PickUp()
    {
        if (IsServer == false)
        {
            return;
        }
        m_isAvailable.Value = false;
        OnPickedUp();
    }

    protected abstract void OnPickedUp();

    public void ToggleSelection(bool isSelected)
    {
        if (m_outline != null)
        {
            m_outline.ToggleOutline(isSelected);
        }
    }
}
