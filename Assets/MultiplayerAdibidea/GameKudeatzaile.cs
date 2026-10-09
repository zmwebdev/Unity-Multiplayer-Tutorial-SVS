using UnityEngine;
using Unity.Netcode;

public class GameKudeatzaile : NetworkBehaviour
{

    public void StartHost()
    {
        // Start the host
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        // Start the client
        NetworkManager.Singleton.StartClient();
    }
}
