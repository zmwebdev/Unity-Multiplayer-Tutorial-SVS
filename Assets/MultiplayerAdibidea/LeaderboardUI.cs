using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>Displays the replicated leaderboard sorted by remaining health (read-only).</summary>
public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text leaderboardText;

    private LeaderboardManager manager;
    private readonly List<LeaderboardEntry> sorted = new();
    private readonly StringBuilder builder = new();

    private void Update()
    {
        // The manager is network-spawned, so it may appear after this UI; bind lazily.
        if (manager == null)
        {
            if (LeaderboardManager.Instance == null || !LeaderboardManager.Instance.IsSpawned)
                return;

            manager = LeaderboardManager.Instance;
            manager.Entries.OnListChanged += OnListChanged;
            Refresh();
        }
    }

    private void OnDestroy()
    {
        if (manager != null && manager.Entries != null)
            manager.Entries.OnListChanged -= OnListChanged;
    }

    private void OnListChanged(NetworkListEvent<LeaderboardEntry> changeEvent) => Refresh();

    private void Refresh()
    {
        if (leaderboardText == null || manager == null)
            return;

        sorted.Clear();
        foreach (LeaderboardEntry entry in manager.Entries)
            sorted.Add(entry);

        sorted.Sort((a, b) =>
        {
            int byHealth = b.Health.CompareTo(a.Health);
            return byHealth != 0 ? byHealth : string.CompareOrdinal(a.Name.ToString(), b.Name.ToString());
        });

        builder.Clear();
        builder.AppendLine("Leaderboard");
        for (int i = 0; i < sorted.Count; i++)
        {
            LeaderboardEntry e = sorted[i];
            string hp = e.Health <= 0f ? "DEAD" : $"{Mathf.CeilToInt(e.Health)} / {Mathf.CeilToInt(e.MaxHealth)}";
            builder.AppendLine($"{i + 1}. {e.Name}  -  {hp}");
        }
        leaderboardText.text = builder.ToString();
    }
}
