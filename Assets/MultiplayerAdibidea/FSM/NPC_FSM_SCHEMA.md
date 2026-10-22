# NPC FSM schema

The NPC finite state machine is implemented by `EnemyStateMachine` and controlled by `EnemyAI`. It switches between four states: Idle, Patrol, Chase, and Attack.

## Structure

```text
EnemyAI
├── EnemyStateMachine
│   └── CurrentState: EnemyState
├── States
│   ├── IdleState
│   ├── PatrolState
│   ├── ChaseState
│   └── AttackState
├── Detection
│   ├── RangeDetector_FSM_Tutorial
│   └── LineOfSightDetector_FSM_Tutorial
└── NavMeshAgent, patrol points, movement/attack settings
```

`EnemyAI` creates the state objects in `Awake()`, initializes the FSM in `Start()` with `IdleState`, and forwards Unity's `Update()` and `FixedUpdate()` calls to the active state.

## State transitions

```text
Start
  └── Idle
       ├── Player in detection range and visible ──> Chase
       └── Idle timer reaches 3 seconds ───────────> Patrol

Patrol
  ├── Player in detection range and visible ───────> Chase
  ├── Reaches waypoint ────────────────────────────> Patrol (next waypoint)
  └── No patrol points ────────────────────────────> Idle

Chase
  ├── Closer player detected and visible ─────────> Switch target immediately
  ├── Player visible and within attack range ──────> Attack
  ├── Player not visible ──────────────────────────> Move to last known position
  │    └── Reached it and unseen for 5 seconds ────> Idle
  └── Player target is null ───────────────────────> Idle

Attack
  ├── Player out of range or not visible ──────────> Chase
  ├── Player target is null ───────────────────────> Idle
  └── Every 1.5 seconds ───────────────────────────> Deal damage
```

## State machine lifecycle

When `ChangeState()` is called, `EnemyStateMachine`:

1. Calls `Exit()` on the current state, if one exists.
2. Sets the new current state.
3. Calls `Enter()` on the new state.

Each state inherits from `EnemyState`. Its `Enter()`, `Update()`, `FixedUpdate()`, and `Exit()` methods define setup, per-frame behavior, physics-frame behavior, and cleanup. States initiate transitions by calling `ChangeState()` on the shared state machine.

## Detection and behavior

- `RangeDetector_FSM_Tutorial` checks for colliders inside its configured sphere radius and detection layer mask. It returns the nearest collider's GameObject.
- `LineOfSightDetector_FSM_Tutorial` uses a raycast and its configured player layer mask to check whether the candidate target is visible.
- `IdleState` stops the NavMeshAgent, waits up to 3 seconds, and checks for a detected, visible player.
- `PatrolState` moves through the configured patrol points at the patrol speed. It returns to Idle if there are no patrol points.
- `ChaseState` and `AttackState` recheck the nearest detected player every frame. If that player is closer than the current target and visible, the NPC switches to that player immediately.
- `ChaseState` moves toward the visible player and updates the last known position. When the player is not visible, it moves toward that last known position; after reaching it, the state returns to Idle once the player has been unseen for 5 seconds.
- `AttackState` stops the agent, faces the player, and attempts an attack every 1.5 seconds while the player remains visible and within attack range. It calls `PlayerHealth.TakeDamage()` if the target has a `PlayerHealth` component.

## Default settings

| Setting | Default |
| --- | ---: |
| Patrol speed | 2 |
| Chase speed | 4 |
| Attack range | 2.5 |
| Attack damage | 10 |
| Idle duration | 3 seconds |
| Time before losing a target | 5 seconds |
| Attack cooldown | 1.5 seconds |

## Unity setup

1. Add `EnemyAI` to the NPC; it requires a `NavMeshAgent`.
2. Assign the `RangeDetector_FSM_Tutorial` and `LineOfSightDetector_FSM_Tutorial` references in the Inspector.
3. Configure the detector layer masks and ranges.
4. Assign patrol point transforms if the NPC should patrol.
5. Add/configure `PlayerHealth` on player targets if attacks should apply damage.

The FSM controller class is named `EnemyStateMachine`; `NPC_FSM` is a descriptive name for this schema, not a class name in the code.
