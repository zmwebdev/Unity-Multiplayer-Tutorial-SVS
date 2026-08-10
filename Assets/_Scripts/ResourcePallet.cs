using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class ResourcePallet : NetworkBehaviour, IInteractable
{

    [SerializeField] private SelectionOutline m_selectionOutline;

    [SerializeField]
    private List<ComponentController> m_componentControllers;

    [SerializeField]
    private ObjectType m_acceptedObjectType;

    private NetworkVariable<int> m_stackedResources = new(0);
    public int StackedResoruces => m_stackedResources.Value;
    public event Action OnPalletFilled;

    [SerializeField] private ItemsAudio m_itemsAudio;
    public void ToggleSelection(bool isSelected)
    {
        if (m_selectionOutline != null)
        {
            m_selectionOutline.ToggleOutline(isSelected);
        }
    }

    public bool Interact(ObjectType objectType)
    {
        if (IsServer == false)
            return false;
        if (objectType != m_acceptedObjectType)
            return false;
        if (m_stackedResources.Value >= m_componentControllers.Count)
            return false;
        PlayerAudioClientRpc();
        m_componentControllers[m_stackedResources.Value].SetEnabled(true);
        m_stackedResources.Value++;
        if (m_stackedResources.Value >= m_componentControllers.Count)
        {
            OnPalletFilled?.Invoke();
        }
        return true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PlayerAudioClientRpc()
    {
        if (m_itemsAudio != null)
            m_itemsAudio.PlaySound();
    }

}
