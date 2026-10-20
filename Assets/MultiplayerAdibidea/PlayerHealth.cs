using System;
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

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth.Value;
    public bool IsDead => currentHealth.Value <= 0f;

    // (previous, current) - useful for UI health bars on every client
    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += HandleHealthChanged;
        UpdateLabel(currentHealth.Value);

        if (IsServer)
            currentHealth.Value = maxHealth;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= HandleHealthChanged;
    }

    // Server only: clients must never modify health directly.
    public void TakeDamage(float amount)
    {
        if (!IsServer || IsDead || amount <= 0f)
            return;

        currentHealth.Value = Mathf.Max(0f, currentHealth.Value - amount);
    }

    public void Heal(float amount)
    {
        if (!IsServer || IsDead || amount <= 0f)
            return;

        currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
    }

    public void ResetHealth()
    {
        if (IsServer)
            currentHealth.Value = maxHealth;
    }

    private void HandleHealthChanged(float previous, float current)
    {
        UpdateLabel(current);
        OnHealthChanged?.Invoke(previous, current);

        if (previous > 0f && current <= 0f)
            OnDied?.Invoke();
    }

    private void UpdateLabel(float health)
    {
        if (healthLabel != null)
            healthLabel.text = $"HP: {Mathf.CeilToInt(health)} / {Mathf.CeilToInt(maxHealth)}";
    }
}
