# Netcode for GameObjects (NGO) – MultiplayerAdibidea scene

Guide to the networking concepts used in `MultiplayerAdibidea.unity` and where each one appears in the scripts.

## 1. Scene overview

| Script | Purpose | NGO concepts |
|---|---|---|
| [SessionManager](SessionManager.cs) | Unity Services sign-in, create/join session through Relay | Multiplayer Services (`WithRelayNetwork`), join code |
| [GameKudeatzaile](GameKudeatzaile.cs) | UI: host/client buttons, player name input | `NetworkManager`, `OnClientConnectedCallback`, `PlayerObject` |
| [NetworkPlayerMovement](NetworkPlayerMovement.cs) | Local input and movement | Ownership (`IsOwner`) |
| [PlayerIzena](PlayerIzena.cs) | Player name label | `NetworkVariable`, `[ServerRpc]` |
| [PlayerKolorea](PlayerKolorea.cs) | Colour per player | `OwnerClientId`, `OnNetworkSpawn` |
| [PlayerHealth](PlayerHealth.cs) | Player health + label | `NetworkVariable`, server authority |
| [SimpleChaseBehavior](SimpleChaseBehavior.cs) | Enemy chases nearest player | Server-only AI, `ConnectedClientsList` |
| [SimpleAttackBehavior](SimpleAttackBehavior.cs) | Enemy attacks nearest player | Server-only logic, calls `TakeDamage` |

`SimplePlayerMovement` and `SimplePatrolBehavior` are the single-player versions and are not networked.

## 2. Basic concepts

- **Host**: server and client in the same process. **Client**: connects to the host (here through Relay with a join code).
- **Server authority**: the server decides game state (health, enemy AI). Clients only send requests and display results.
- **NetworkManager**: singleton (`NetworkManager.Singleton`) that starts/stops the network and tracks clients.
- **NetworkObject**: required on every prefab that is synchronised. Must be registered in `DefaultNetworkPrefabs.asset`.
- **NetworkBehaviour**: `MonoBehaviour` replacement for scripts that need network features (`IsServer`, `IsOwner`, RPCs, `NetworkVariable`).

## 3. Roles and ownership

Useful properties inside a `NetworkBehaviour`:

| Property | Meaning |
|---|---|
| `IsServer` | This instance runs on the server/host |
| `IsClient` | This instance runs on a client (host is both) |
| `IsHost` | Server and client at once |
| `IsOwner` | The local client owns this object |
| `OwnerClientId` | Id of the owning client |

Examples in this scene:
- `NetworkPlayerMovement`: `if (!IsOwner) return;` so each client moves only its own player.
- `PlayerKolorea`: `OwnerClientId % materials.Length` chooses a material, giving each player a different colour.
- Enemies: `if (!IsServer) enabled = false;` so AI runs only on the server.

## 4. Lifecycle

```csharp
public override void OnNetworkSpawn()   // object is ready on the network: subscribe, initialise
public override void OnNetworkDespawn() // unsubscribe
```

Use `OnNetworkSpawn` instead of `Awake`/`Start` for anything that depends on `IsServer`/`IsOwner`, since those values are not valid earlier.

## 5. NetworkVariable

Synchronised state, automatically sent from the writer to everyone, including late joiners.

```csharp
private readonly NetworkVariable<FixedString64Bytes> playerName = new(
    default,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);
```

- **Read permission**: `Everyone` or `Owner`.
- **Write permission**: `Server` (safest) or `Owner`.
- **OnValueChanged**: `(previous, current)` callback fired on every machine that receives the change.
- Allowed types: unmanaged/value types (`int`, `float`, `bool`, enums, structs, `FixedString`). Do not use `string`; use `FixedString32/64/128Bytes`.
- Use for **persistent state**: names, health, scores.

In this scene:
- `PlayerIzena.playerName` – player name, shown in the label.
- `PlayerHealth.currentHealth` – server-written, read by all; `OnValueChanged` updates the HP label.

## 6. RPCs

An RPC is a method call sent over the network. Use for **events/actions**, not persistent state.

| Type | Called from | Runs on |
|---|---|---|
| `[ServerRpc]` | Client (owner by default) | Server |
| `[ClientRpc]` | Server | All clients |

Rules:
- Method name must end with `ServerRpc` / `ClientRpc`.
- Parameters must be serialisable (primitives, `FixedString`, structs, arrays of those).
- `[ServerRpc(RequireOwnership = false)]` allows non-owners to call it. Validate inputs on the server.

In this scene:

```csharp
// PlayerIzena
public void SetMyName(string name)
{
    if (IsOwner) SubmitNameServerRpc(name);
}

[ServerRpc]
private void SubmitNameServerRpc(string name)
{
    playerName.Value = new FixedString64Bytes(name); // server writes, NetworkVariable replicates
}
```

The client cannot write `playerName` directly (write permission is `Server`), so it asks the server through an RPC.

### Why `PlayerHealth.TakeDamage` is not an RPC

`TakeDamage` is only called from `SimpleAttackBehavior`, which runs only on the server. It is a plain server-side method (`if (!IsServer) return;`) that writes a `NetworkVariable`; replication to clients is automatic. Making it a `[ServerRpc]` would be unnecessary and would let clients call damage. Use a `[ServerRpc]` only if a **client** must trigger the action (e.g. player attacks an enemy), and a `[ClientRpc]` for one-off effects (sound, animation, particles).

### NetworkVariable or RPC?

| Need | Use |
|---|---|
| State that late joiners must see (health, name, score) | `NetworkVariable` |
| One-off event (play sound, show effect) | `[ClientRpc]` |
| Client asks server to do something | `[ServerRpc]` |

## 7. Connection flow in this scene

1. `SessionManager.Start` initialises Unity Services and signs in anonymously.
2. **Host**: `GameKudeatzaile.StartHost` → `SessionManager.StartSessionAsHost` creates a Relay session and shows the join code.
3. **Client**: `GameKudeatzaile.StartClient` → `StartSessionAsClient(joinCode)` joins the session.
4. NGO spawns the Player prefab for every connected client.
5. `GameKudeatzaile` waits for `OnClientConnectedCallback`, gets `LocalClient.PlayerObject` and calls `PlayerIzena.SetMyName`.
6. While connected, editing the name input sends the new name again, and every client's label updates through the `NetworkVariable`.

## 8. Enemy AI (server-authoritative)

- `SimpleChaseBehavior` and `SimpleAttackBehavior` are `NetworkBehaviour`s that disable themselves on clients.
- Target is picked at runtime: `NetworkManager.Singleton.ConnectedClientsList` → each `client.PlayerObject` → nearest by distance.
- Clients see the enemy move through **NetworkTransform** (position/rotation sync). The `NavMeshAgent` is disabled on clients.
- Always null-check `NetworkManager.Singleton` and `PlayerObject`; players can disconnect at any time.
- Damage: `PlayerHealth.TakeDamage(amount)` on the server → `currentHealth` changes → all clients update their HP label.

## 9. Prefab checklist

**Player prefab**
- `NetworkObject`
- `NetworkTransform` (use *Client Authority* / owner-authoritative if the owner moves it, e.g. `ClientNetworkTransform`)
- `NetworkPlayerMovement`, `PlayerInput`
- `PlayerIzena` (+ name label), `PlayerKolorea` (+ materials), `PlayerHealth` (+ health label)
- Registered as the **Player Prefab** in `NetworkManager`

**Enemy prefab**
- `NetworkObject`, `NetworkTransform` (server-authoritative), `NavMeshAgent`
- `SimpleChaseBehavior`, `SimpleAttackBehavior`
- Added to `DefaultNetworkPrefabs.asset` and spawned by the server (`Instantiate` + `NetworkObject.Spawn()`), or placed in the scene as an in-scene NetworkObject

**Scene**
- `NetworkManager` with transport (Unity Transport / Relay)
- Baked NavMesh for the enemies

## 10. Common mistakes

- Writing a `NetworkVariable` from a client that has no write permission (error in Console).
- Using `IsOwner`/`IsServer` in `Awake`/`Start` (not set yet) – use `OnNetworkSpawn`.
- Forgetting to unsubscribe `OnValueChanged` in `OnNetworkDespawn`.
- Prefab without `NetworkObject` or not in the network prefabs list.
- Running AI or gameplay logic on every client instead of only the server.
- Accessing `NetworkManager.Singleton.LocalClient.PlayerObject` before it spawns, or after disconnect (null).
- Trusting client input in a `[ServerRpc]` without validation.

## 11. Testing

- Unity Editor as host + a build (`Build/`) as client, or use **Multiplayer Play Mode** (Window → Multiplayer Play Mode).
- Host creates the session; copy the join code into the client input and press join.
- Check: names appear above all players, health decreases when the enemy attacks, enemies move identically on all clients.

## 12. References

- [NGO documentation](https://docs-multiplayer.unity3d.com/netcode/current/about/)
- [NetworkVariable](https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable/)
- [RPCs](https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/rpc/)
- See also `MULTIPLAYER_DEVELOPMENT_GUIDE.md` in the repository root.
