using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Server-owned leaderboard. Only server-side code (PlayerHealth / PlayerIzena) writes to it;
/// clients just read the replicated list. Place on a scene NetworkObject.
/// </summary>
public class LeaderboardManager : NetworkBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    public NetworkList<LeaderboardEntry> Entries { get; private set; }

    private void Awake()
    {
        Instance = this;
        Entries = new NetworkList<LeaderboardEntry>(
            null,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Entries.Clear();
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;

        base.OnNetworkDespawn();
    }

    public override void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        Entries?.Dispose();
        base.OnDestroy();
    }

    public void SetName(ulong clientId, string playerName)
    {
        if (!IsServer) return;
        int i = GetOrCreateIndex(clientId);
        LeaderboardEntry entry = Entries[i];
        entry.Name = new FixedString64Bytes(playerName);
        Entries[i] = entry;
    }

    public void SetHealth(ulong clientId, float health, float maxHealth)
    {
        if (!IsServer) return;
        int i = GetOrCreateIndex(clientId);
        LeaderboardEntry entry = Entries[i];
        entry.Health = health;
        entry.MaxHealth = maxHealth;
        Entries[i] = entry;
    }

    public void Remove(ulong clientId)
    {
        if (!IsServer) return;
        int i = IndexOf(clientId);
        if (i >= 0)
            Entries.RemoveAt(i);
    }

    private void HandleClientDisconnected(ulong clientId) => Remove(clientId);

    private int GetOrCreateIndex(ulong clientId)
    {
        int i = IndexOf(clientId);
        if (i >= 0) return i;

        Entries.Add(new LeaderboardEntry
        {
            ClientId = clientId,
            Name = new FixedString64Bytes($"Player {clientId}")
        });
        return Entries.Count - 1;
    }

    private int IndexOf(ulong clientId)
    {
        for (int i = 0; i < Entries.Count; i++)
            if (Entries[i].ClientId == clientId)
                return i;
        return -1;
    }
}
