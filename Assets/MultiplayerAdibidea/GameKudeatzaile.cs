using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using TMPro;

public class GameKudeatzaile : NetworkBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private TMP_InputField joinCode;
    [SerializeField] private TMP_InputField playerNameInput;

    private NetworkManager networkManager;
    private string pendingPlayerName;

    public async void StartHost()
    {
        if (!PreparePlayerName())
            return;

        await sessionManager.StartSessionAsHost();
        TrySetLocalPlayerName();
    }

    public async void StartClient()
    {
        if (!PreparePlayerName())
            return;

        Debug.Log($"Joining session with code: {joinCode.text}");
        await sessionManager.StartSessionAsClient(joinCode.text.Trim());
        TrySetLocalPlayerName();
    }

    private bool PreparePlayerName()
    {
        if (playerNameInput == null)
        {
            Debug.LogError("GameKudeatzaile: Player name input is not assigned.", this);
            return false;
        }

        pendingPlayerName = playerNameInput.text;
        PlayerIzena.DesiredName = pendingPlayerName;

        networkManager = NetworkManager.Singleton;
        if (networkManager == null)
        {
            Debug.LogError("GameKudeatzaile: NetworkManager.Singleton was not found.", this);
            return false;
        }

        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        return true;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (networkManager != null && clientId == networkManager.LocalClientId)
            TrySetLocalPlayerName();
    }

    private void TrySetLocalPlayerName()
    {
        if (networkManager == null || networkManager.LocalClient == null)
            return;

        NetworkObject playerObject = networkManager.LocalClient.PlayerObject;
        if (playerObject == null)
            return;

        PlayerIzena playerIzena = playerObject.GetComponent<PlayerIzena>();
        if (playerIzena == null)
        {
            Debug.LogError("The local Player object is missing the PlayerIzena component.", playerObject);
            networkManager.OnClientConnectedCallback -= OnClientConnected;
            return;
        }

        playerIzena.SetMyName(pendingPlayerName);
        networkManager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnEnable()
    {
        if (playerNameInput != null)
            playerNameInput.onValueChanged.AddListener(OnPlayerNameInputChanged);
    }

    // Propagates the local edit to the server; other clients only receive it via the NetworkVariable label.
    private void OnPlayerNameInputChanged(string newName)
    {
        pendingPlayerName = newName;
        PlayerIzena.DesiredName = newName;

        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || !manager.IsClient || manager.LocalClient == null)
            return;

        NetworkObject playerObject = manager.LocalClient.PlayerObject;
        if (playerObject != null && playerObject.TryGetComponent(out PlayerIzena playerIzena))
            playerIzena.SetMyName(newName);
    }

    private void OnDisable()
    {
        if (playerNameInput != null)
            playerNameInput.onValueChanged.RemoveListener(OnPlayerNameInputChanged);

        if (networkManager != null)
            networkManager.OnClientConnectedCallback -= OnClientConnected;
    }
}
