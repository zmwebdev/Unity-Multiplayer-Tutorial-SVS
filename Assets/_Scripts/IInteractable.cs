using Unity.Netcode;
using UnityEngine;

public interface IInteractable
{
    void ToogleSelection(bool isSelected);
    NetworkObject NetworkObject { get; }
        
}
