using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using TMPro;

public class GameKudeatzaile : NetworkBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private TMP_InputField joinCode;

    public async void StartHost()
    {
        // Start the host
        //NetworkManager.Singleton.StartHost();

        //
        await sessionManager.StartSessionAsHost();
    }

    public async void StartClient()
    {
        // Start the client
        //NetworkManager.Singleton.StartClient();

        // session manager erabibil join egin

        Debug.Log($"Joining session with code: {joinCode.text}");
        await sessionManager.StartSessionAsClient(joinCode.text.Trim());
    }

     
}
