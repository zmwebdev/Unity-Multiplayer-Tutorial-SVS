using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-authoritative player health. Add to the Player prefab (next to NetworkObject).
/// </summary>
public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private TMP_Text healthLabel; // Optional: e.g. a world-space text above the player

    private readonly NetworkVariable<float> currentHealth = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += HandleHealthChanged;
        UpdateLabel(currentHealth.Value);

        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            LeaderboardManager.Instance?.SetHealth(OwnerClientId, currentHealth.Value, maxHealth);
        }
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= HandleHealthChanged;

        if (IsServer)
            LeaderboardManager.Instance?.Remove(OwnerClientId);
    }

    // Server only: clients must never modify health directly.
    public void TakeDamage(float amount)
    {
        if (!IsServer || currentHealth.Value <= 0f || amount <= 0f)
            return;

        float before = currentHealth.Value;
        currentHealth.Value = Mathf.Max(0f, before - amount);

        float realDamage = before - currentHealth.Value; // doesn't count damage beyond 0 HP
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(-Mathf.RoundToInt(realDamage));
    }

    private void HandleHealthChanged(float _, float current)
    {
        UpdateLabel(current);

        if (IsServer)
            LeaderboardManager.Instance?.SetHealth(OwnerClientId, current, maxHealth);
    }

    private void UpdateLabel(float health)
    {
        if (healthLabel != null)
            healthLabel.text = $"HP: {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}";
    }
}
