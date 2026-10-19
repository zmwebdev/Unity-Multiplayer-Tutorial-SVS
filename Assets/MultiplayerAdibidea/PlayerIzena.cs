using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerIzena : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameLabel;

    private readonly NetworkVariable<FixedString64Bytes> playerName = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;
        UpdateLabel(playerName.Value);
    }

    public override void OnNetworkDespawn()
    {
        playerName.OnValueChanged -= OnNameChanged;
    }

    // Call this on the owning player's object once their name is available.
    public void SetMyName(string name)
    {
        if (IsOwner)
            SubmitNameServerRpc(name);
    }

    [ServerRpc]
    private void SubmitNameServerRpc(string name)
    {
        playerName.Value = new FixedString64Bytes(name);
    }

    private void OnNameChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        UpdateLabel(current);
    }

    private void UpdateLabel(FixedString64Bytes name)
    {
        if (nameLabel != null)
            nameLabel.text = name.ToString();
    }
}