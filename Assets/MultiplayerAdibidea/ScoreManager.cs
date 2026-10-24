using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public static ScoreManager Instance { get; private set; }

    public NetworkVariable<int> TotalScore = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        TotalScore.OnValueChanged += OnScoreChanged;
        UpdateText(TotalScore.Value); // initial value, also for late joiners
    }

    public override void OnNetworkDespawn()
    {
        TotalScore.OnValueChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(int oldValue, int newValue) => UpdateText(newValue);

    private void UpdateText(int value)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {value}";
    }

    // Server-side API (enemy dies, etc.)
    public void AddScore(int amount)
    {
        if (!IsServer) return;
        TotalScore.Value += amount;
    }
}