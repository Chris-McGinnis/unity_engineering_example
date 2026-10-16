# Unity Engineering Example

Companion project for a talk on four patterns that keep Unity code decoupled and easy to change:

1. [Interfaces](#1-interfaces)
2. [Observer Pattern](#2-observer-pattern)
3. [Finite State Machines](#3-finite-state-machines)
4. [Factory Pattern](#4-factory-pattern)

All code lives under `Assets/Scripts`. Factory assets live under `Assets/Factories` and prefabs under `Assets/Prefabs`.

---

## 1. Interfaces

**Idea:** an interface is a contract ("anything that can take damage"). Callers depend on the contract, not on concrete classes, so new types plug in without changing existing code.

| Where | What to look at |
|---|---|
| `IDamageable.cs` | The contract: `CurrentHealth` and `TakeDamage(float)`. |
| `Actors/Projectile.cs` | Hits anything via `TryGetComponent<IDamageable>`. It doesn't know if it hit an enemy, crate, barrel or the player. |
| `Actors/EnemyBase.cs` | Implements it, and also *uses* it for contact damage on the player (`GetComponentInParent<IDamageable>`). |
| `PlayerHealthSystem.cs`, `Actors/DestructibleCrate.cs`, `Actors/ExplosiveBarrel.cs` | Other implementers, each with different behavior (die, break and drop loot, start a fuse). |
| `Actors/FlyingEnemy.cs` | `IAttackBehavior` with `MeleeAttack` / `RangedAttack`, swappable in the Inspector via `[SerializeReference]`. |
| `StateMachine/StateMachine.cs` | `IState` is the contract the state machine runs. |

**Talking points**
- Adding a new damageable object means implementing one interface. `Projectile` never changes.
- Unity can't serialize a plain interface field (see the commented `IDamageable target` in `Factory/EnemyFactory.cs`). `[SerializeReference]` on `FlyingEnemy.attackBehavior` is the workaround for plain C# classes. For `MonoBehaviour` implementers, use `GetComponent<T>()`.

---

## 2. Observer Pattern

**Idea:** a *subject* raises an event and any number of *observers* react. The subject never references its observers, so UI, audio and spawners can listen without being wired into gameplay code.

| Flavor | Subject | Observer(s) |
|---|---|---|
| C# `static event` | `PlayerHealthSystem` (`OnHealthChanged`, `OnDied`) | `UI/HealthBarUI.cs`, `Actors/Player.cs` (tints the sprite), `Factory/LevelSpawner.cs` (stops spawning on death) |
| C# instance event | `Actors/EnemyBase.cs` (`ReturnedToPool`) | `Factory/LevelSpawner.cs` tracks active enemies to know when a wave is cleared |
| ScriptableObject event channel | `Events/GameEventSO.cs`, raised by `ZoneTrigger.cs` | `UI/ZoneTriggerUI.cs` |
| `UnityEvent` (Inspector wired) | `ZoneTrigger.cs` (`OnTriggerEnterUnityEvent`) | Whatever is hooked up in the Inspector |
| Input System callbacks | `InputAction.performed` | `Actors/Player.cs` (`HandleMove`, `HandleJump`, `HandleShoot`) |

**Talking points**
- `PlayerHealthSystem` has zero knowledge of the health bar. Delete the UI and nothing breaks.
- Always unsubscribe (`OnDisable` / `OnDestroy`). See `HealthBarUI` and `LevelSpawner`.
- Unsubscribing needs the *same* delegate. `ZoneTriggerUI` uses a method group for this reason. Unsubscribing a fresh lambda silently does nothing and leaks.
- ScriptableObject channels let objects in different scenes or prefabs communicate without a direct reference.

---

## 3. Finite State Machines

**Idea:** an object is in exactly one state at a time. Each state owns its behavior and its transitions, which replaces large `if/else` blocks over flags.

| Where | What to look at |
|---|---|
| `StateMachine/StateMachine.cs` | `IState` (`Enter` / `Update` / `Exit`) and `StateMachine`, whose `ChangeState` guarantees Exit-then-Enter ordering. |
| `Factory/IState.cs` | The abstract `State` base class (gives states a `Machine` reference and empty virtual methods). Note it lives in the `Factory` folder. |
| `StateMachine/PlayerIdleState.cs`, `PlayerRunState.cs`, `PlayerJumpState.cs` | Class-per-state. Each state decides the next one. |
| `Actors/Player.cs` | Owns the machine, initializes it with `PlayerIdleState`, and calls `stateMachine.Update()`. |
| `Actors/ExplosiveBarrel.cs` | A lightweight alternative: an `enum` plus a `switch` in `Update` (Idle -> Countdown -> Exploding). |

**Talking points**
- Two approaches to compare. The enum/`switch` version (barrel) is quick and fine for a few simple states. The class-per-state version (player) scales better, because each state is small, testable and has its own `Enter`/`Exit` hooks.
- States run their own transitions, so the owning `Player` stays free of state logic.

---

## 4. Factory Pattern

**Idea:** ask a factory for an object instead of calling `Instantiate` and configuring it yourself. The factory hides construction, configuration and pooling.

| Where | What to look at |
|---|---|
| `Factory/EnemyFactory.cs` | Abstract ScriptableObject factory. `CreateEnemy()` returns a configured, pooled enemy. |
| `Factory/BasicEnemyFactory.cs`, `Factory/FlyingEnemyFactory.cs` | Concrete factories. They only declare which enemy type they accept. |
| `Factory/PooledPrefabFactory.cs` | Template-method base class for non-enemy objects. It owns the pool and subclasses fill in the type-specific hooks. |
| `Factory/ProjectileFactory.cs`, `Factory/CrateFactory.cs` | Pooled factories. `CrateFactory` also picks random drops. |
| `Factory/BarrelFactory.cs` | A minimal factory with no pooling (barrels are few). |
| `Factory/EnemySpawner.cs`, `LevelSpawner.cs`, `EnvironmentObjectSpawner.cs` | Clients. They hold factory assets and call `Create...()`, and never touch prefabs or `Instantiate`. |
| `Actors/Player.cs` | Fires projectiles via `projectileFactory.CreateProjectile(...)`. |
| `Assets/Factories/` | The factory assets, where designers tune health, speed and pool size. |

**Talking points**
- The "armored enemy" is just `BasicEnemy` with `damageMultiplier` set to 0.5 on its prefab, plus another `BasicEnemyFactory` asset. It's a data change, not new code.
- A wave is data: a list of (factory, count) entries in `LevelSpawner`. Adding an enemy type to a level needs no code changes.
- Pools live on the factory (one shared pool per asset). Objects like `Projectile` and `EnemyBase` receive an `IObjectPool` through `SetPool` and release themselves when finished.

---

## How the patterns work together

`Player` fires -> a **factory** (`ProjectileFactory`) hands back a pooled `Projectile` -> it hits something through the **interface** (`IDamageable`) -> `PlayerHealthSystem` / `EnemyBase` raise **events** -> `HealthBarUI` and `LevelSpawner` react as **observers** -> the player's movement is driven by an **FSM** the whole time.