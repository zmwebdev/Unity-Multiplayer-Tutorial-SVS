# MultiplayerAdibidea: a first look at Netcode for GameObjects

This guide introduces the multiplayer setup in this Unity scene. It is written for students who already know their way around Unity scenes, GameObjects, components, prefabs, and C# scripts, but are new to networking.

The project uses **Netcode for GameObjects (NGO) 2.13.3** for gameplay networking and **Unity Multiplayer Services** to create and join sessions through Relay. The scene and scripts referenced below are in this folder.

## What you will learn

By exploring this example, you will learn how to:

- start a host or join one as a client;
- let NGO create a player object for each connected player;
- decide which machine controls an object;
- synchronize state that every player needs to see;
- send a request or one-time event between clients and the server;
- keep server-owned gameplay, such as enemy attacks and health, consistent.

## Run the scene

1. Open the Unity project and open `MultiplayerAdibidea.unity`.
2. To connect over the internet, sign in to Unity and make sure the project is linked to a Unity Cloud project with Authentication and Multiplayer/Relay configured. This scene signs players in anonymously; it still needs an internet connection.
3. Start one instance and select **Host**. The host creates a Relay session and displays a join code.
4. Start a second instance, enter the host's join code, and select **Client**. You can use a standalone build for the second instance or Unity's Multiplayer Play Mode if it is installed and configured.
5. Check the Unity Console for connection or setup errors.

The `SessionManager` handles Unity Services sign-in and creating/joining Relay sessions. The `GameKudeatzaile` scene object connects the UI buttons to those operations. Relay helps players connect; it does not decide game rules or synchronize gameplay state. NGO handles those parts.

## The essential idea: more than one copy of the game

In a network session, each player runs a copy of the game. Each copy has its own scene, GameObjects, scripts, and UI. NGO sends selected information between those copies so that they can present one shared game.

### Host, server, and client

- A **server** controls shared game state and decides whether gameplay requests are valid.
- A **client** is a player connected to that server.
- A **host** is both the server and a local client in one application.

In NGO, the host has both server and client roles. Do not assume that a GameObject or a variable is automatically shared just because it exists in a scene.

### `NetworkManager`: the session's networking hub

Each session needs one active `NetworkManager`. It starts and stops NGO, tracks connected clients, and holds settings such as the player prefab and transport. This scene's NetworkManager uses Unity Transport; the session setup configures Relay for internet connections.

### `NetworkObject`: the identity NGO synchronizes

Add a `NetworkObject` to a prefab or scene object that must exist as a networked object. It gives that object a network identity so NGO can spawn, track, and despawn its corresponding instances on connected machines.

Network prefabs must be registered with the NetworkManager. In this scene, `Player.prefab` is assigned as the player prefab and is also included in `NetworkPrefabsListAdibidekoak.asset`. When a client joins, NGO creates a player object for that client.

### `NetworkBehaviour`: a script with networking features

Use `NetworkBehaviour` instead of `MonoBehaviour` when a script needs NGO features such as `IsServer`, `IsOwner`, `NetworkVariable`, or RPCs. The script must be on the same GameObject as a `NetworkObject` (or on one of its children, as supported by NGO).

Ordinary Unity scripts and UI do not need to inherit from `NetworkBehaviour` unless they use those networking features.

## Who controls an object?

Two checks you will use often are:

| Check | Meaning |
| --- | --- |
| `IsServer` | This copy is running the server role. True on the host. |
| `IsClient` | This copy is running a client role. True on the host as well. |
| `IsHost` | This copy is both server and client. |
| `IsOwner` | This copy belongs to the locally controlled client. |
| `OwnerClientId` | The network ID of the client that owns the object. |

Ownership answers **which client owns this object?** It does not automatically make that client trustworthy or give it permission to change every piece of shared state. Server authority means the server decides important game state.

In `NetworkPlayerMovement`, the input component is enabled only for the owning player, and movement is applied only when `IsOwner` is true. The player prefab's `NetworkTransform` is configured for owner authority so that the owner's movement is synchronized. The enemy scripts instead run their AI on the server.

### Wait for the network spawn lifecycle

`Awake` and `Start` are normal Unity lifecycle methods. They can run before an object has been spawned into the network, so avoid using them to make decisions that depend on `IsServer`, `IsOwner`, or the object's network state.

NGO provides network lifecycle methods for that:

```csharp
public override void OnNetworkSpawn()
{
    // The object has been spawned by NGO. Check ownership or subscribe to network state here.
}

public override void OnNetworkDespawn()
{
    // Unsubscribe from network state or clean up network-specific work here.
}
```

Unsubscribe from events in `OnNetworkDespawn` when you subscribed in `OnNetworkSpawn`. This prevents stale subscriptions when an object is despawned and spawned again.

## Synchronize state with `NetworkVariable`

A `NetworkVariable` stores a value that should stay synchronized, such as a player's health or a shared score. NGO sends its current value to connected players, including clients that join later. This makes it suitable for **state**, rather than a one-time notification.

For example, `PlayerHealth` stores health in a server-write, everyone-read `NetworkVariable<float>`. The server changes the value when damage is taken, and each client updates its local health label when the value changes.

Important points:

- Choose read and write permissions deliberately. A common pattern is **everyone can read; only the server can write**.
- Subscribe to `OnValueChanged` to update local UI or presentation, and unsubscribe when the network object despawns.
- Initialize UI from the current value as well as listening for changes. A client may spawn after the value was first set.
- NGO network variables support specific serializable types. For text, use a supported fixed string type such as `FixedString64Bytes`, not a regular C# `string`.

Examples in this project:

- `PlayerHealth` synchronizes player health.
- `ScoreManager` synchronizes the shared score and updates the local score text on each machine.
- `PlayerIzena` contains an example of synchronizing a player name.

**Project setup note:** `PlayerIzena.cs` is present, but `Player.prefab` does not currently have that component or a name label assigned. To use the name feature, add and configure the component on the player prefab, then assign its label in the Inspector.

## Send requests and events with RPCs

An **RPC** (remote procedure call) asks NGO to run a method on another machine. Use one when something happens, or when a client needs to ask the server to perform an action. Do not use an RPC as a replacement for persistent state that late joiners must see.

NGO 2.x supports the unified `[Rpc]` API. For example, a client can request that the server perform an action using an RPC targeted at the server. The server should validate the request before changing important game state. The exact permissions and target should match the action and object ownership.

All RPCs in this scene use the unified `[Rpc]` form; there are no legacy `[ServerRpc]` or `[ClientRpc]` methods. In `PlayerIzena`, the owning client submits a name to the server with `[Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]`, and the server writes it to a `NetworkVariable` that delivers the name to everyone. `ScoreManager` also shows a server-targeted `AddScoreRpc` and a `ScoreFeedbackRpc` sent to clients and the host. Name RPC methods with the `Rpc` suffix, and choose the target and invoke permission to suit the operation.

| Need | Suitable tool |
| --- | --- |
| Shared value that must remain current, including for late joiners | `NetworkVariable` |
| Client asks the server to do something | Server-targeted RPC, with server-side validation |
| One-time effect or notification for connected clients | Client-targeted RPC |

For example, `PlayerHealth.TakeDamage` is a normal method rather than an RPC because the enemy attack logic already runs on the server. The server changes health directly; the `NetworkVariable` distributes the result.

## Follow the flow through this example

1. `SessionManager` initializes Unity Services and signs in anonymously.
2. The host button calls `SessionManager.StartSessionAsHost`, which creates a Relay session and displays its join code.
3. A client enters that code and joins through `SessionManager.StartSessionAsClient`.
4. NGO connects the players and spawns a `Player.prefab` for each client.
5. Each copy has the same networked player object, but only its owner reads that player's input.
6. The owner-authoritative `NetworkTransform` synchronizes the player's movement to the other connected copies.
7. Server-side enemy logic can select connected players, attack them, and change their health. NGO replicates health and score state to the clients.

Useful files to read in order:

1. `SessionManager.cs` — Unity Services, Relay, and session creation/joining.
2. `GameKudeatzaile.cs` — UI calls that start the host or client.
3. `Player.prefab` and `NetworkPlayerMovement.cs` — player network object, ownership, input, and transform synchronization.
4. `PlayerHealth.cs` and `ScoreManager.cs` — server-written state and client-side display.
5. `SimpleChaseBehavior.cs` and `SimpleAttackBehavior.cs` — server-only enemy behavior.

## Testing and troubleshooting

Try connecting two instances and observe the same player objects, movement, enemy behavior, health, and score in both. Use the Unity Console to investigate errors on either instance.

Common problems:

- **The player does not appear:** Check that the NetworkManager is active, `Player.prefab` is assigned as its Player Prefab, and the prefab has a `NetworkObject`.
- **Another client cannot connect:** Check internet access, Unity Services project configuration, Authentication/Relay setup, and the join code.
- **The player moves locally but not remotely:** Check the `NetworkTransform` authority settings and that it is on the networked player object.
- **A client tries to write server-owned state:** Send a request to the server instead. Only the permitted writer can change a `NetworkVariable`.
- **A network check behaves unexpectedly in `Awake` or `Start`:** Move logic that depends on ownership or server/client role to `OnNetworkSpawn`.
- **The local player reference is null:** The player may not have spawned yet, or may have disconnected. Check after the connection/spawn callback and handle a missing object.
- **A networked prefab cannot spawn:** Make sure it has a `NetworkObject` and is registered with the NetworkManager.

## References

- [Netcode for GameObjects manual (2.13)](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html)
- [NetworkVariables](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/basics/networkvariable.html)
- [RPCs](https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/advanced-topics/message-system/rpc.html)
- [Unity Multiplayer Services documentation](https://docs.unity.com/mps)
- [Unity Multiplayer Play Mode documentation](https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@2.0/manual/index.html)
