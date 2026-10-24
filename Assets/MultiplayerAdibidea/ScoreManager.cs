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

    // Client asks the server to add score
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void AddScoreRpc(int amount)
    {
        TotalScore.Value += amount;
    }

    // Server tells every client about a one-off event
    [Rpc(SendTo.ClientsAndHost)]
    private void ScoreFeedbackRpc(int amount)
    {
        Debug.Log($"+{amount} points!");
    }

    public void AddScoreWithFeedback(int amount)
    {
        if (!IsServer) return;
        TotalScore.Value += amount;
        ScoreFeedbackRpc(amount);
    }
}