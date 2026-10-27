using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerIzena : NetworkBehaviour
{
    private const int MaxNameLength = 20;

    // Latest name typed locally; sent automatically when the owned player spawns.
    public static string DesiredName { get; set; }

    [SerializeField] private TMP_Text nameLabel;

    private readonly NetworkVariable<FixedString64Bytes> playerName = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnNameChanged;
        UpdateLabel(playerName.Value);

        if (IsServer)
        {
            string initial = $"Player {OwnerClientId}";
            playerName.Value = new FixedString64Bytes(initial);
            LeaderboardManager.Instance?.SetName(OwnerClientId, initial);
        }

        // The name may have been typed before this player object existed.
        if (IsOwner && !string.IsNullOrWhiteSpace(DesiredName))
            SubmitNameServerRpc(DesiredName);
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
        // Never trust the client: sanitize and fall back to a default name.
        name = name == null ? string.Empty : name.Trim();
        if (name.Length == 0)
            name = $"Player {OwnerClientId}";
        if (name.Length > MaxNameLength)
            name = name.Substring(0, MaxNameLength);

        playerName.Value = new FixedString64Bytes(name);
        LeaderboardManager.Instance?.SetName(OwnerClientId, name);
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