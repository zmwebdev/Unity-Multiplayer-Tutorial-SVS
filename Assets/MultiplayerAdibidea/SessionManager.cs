using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;

public class SessionManager : MonoBehaviour
{
    async void Start()
    {
	    try
	    {
	     await UnityServices.InitializeAsync();
	     await AuthenticationService.Instance.SignInAnonymouslyAsync();
	     Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");

         await StartSessionAsHost();
	    }
	    catch (Exception e)
	    {
	     Debug.LogException(e);
	    }
    }

    async Task StartSessionAsHost()
    {
        var options = new SessionOptions
        {
            MaxPlayers = 2
        }.WithRelayNetwork(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay
        
        var session = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {session.Id} created! Join code: {session.Code}");
    }    

}
