using Unity.Netcode;
using UnityEngine;

public class PlayerKolorea : NetworkBehaviour
{
    [SerializeField] private Material[] materials;

    public override void OnNetworkSpawn()
    {
        Renderer playerRenderer = GetComponent<Renderer>();

        if (playerRenderer == null)
        {
            Debug.LogError("PlayerKolorea: No Renderer found on the player.", this);
            return;
        }

        if (materials == null || materials.Length == 0)
        {
            Debug.LogError("PlayerKolorea: No materials assigned.", this);
            return;
        }

        int index = (int)(OwnerClientId % (ulong)materials.Length);
        Material selectedMaterial = materials[index];

        if (selectedMaterial == null)
        {
            Debug.LogError($"PlayerKolorea: No material assigned at index {index}.", this);
            return;
        }

        playerRenderer.sharedMaterial = selectedMaterial;
    }
}
