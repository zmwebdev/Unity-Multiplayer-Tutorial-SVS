using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    void ToggleSelection(bool isSelected);
    NetworkObject NetworkObject { get; }
        
}
