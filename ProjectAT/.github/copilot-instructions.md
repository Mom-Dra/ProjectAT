# ProjectAT Copilot Instructions

## Project Overview
- **Engine**: Unity 2022+ (implied)
- **Language**: C#
- **Networking**: Unity Netcode for GameObjects (NGO)
- **Architecture**: Component-based with specific patterns for Networking and Entity management.

## Architecture & Patterns

### Networking (Netcode for GameObjects)
- **Object Pooling**: Use `NetworkObjectPool` for spawning networked entities (projectiles, enemies).
  - **Pattern**: Register prefabs in `NetworkObjectPool` inspector list.
  - **Spawning**: `NetworkObjectPool.Instance.GetNetworkObject(prefab, pos, rot)` (Server only).
  - **Despawning**: `NetworkObjectPool.Instance.ReturnNetworkObject(netObj, prefab)`.
- **Server Authority**: Logic modifying state (health, death) should generally run on the Server (`// OnlyServer` comments indicate this intent).
- **Inheritance**: Networked components must inherit from `NetworkBehaviour`, not just `MonoBehaviour`.

### Entities & Health
- **Interfaces**: Use `IDamageable` for any object that takes damage.
- **Status**: `EntityStatus` holds runtime data (`CurrentHp`, `IsDead`) and references `EntityInitialStatus` (ScriptableObject) for config.
- **Base Classes**: `LivingEntity` exists as a base for damageable objects, but check for overlap with `EntityStatus`.

### Code Style & Conventions
- **Inspector Exposure**: Use `[SerializeField] private` for fields exposed to the Editor. Avoid public fields unless necessary.
- **Lifecycle**: Ensure correct casing for Unity messages (`Start()`, `Awake()`, `OnEnable()`).
- **Properties**: Be careful with property definitions to avoid recursive calls (e.g., `get { return Property; }`).
- **Debugging**: `Debug.Log` is acceptable for development but should be cleaned up in production paths.

## Common Pitfalls
- **Recursive Properties**: Watch out for `get { return Name; }` where `Name` is the property itself.
- **Lifecycle Typos**: `start()` will not be called by Unity; use `Start()`.
- **Network vs Local**: `LivingEntity` currently inherits `MonoBehaviour` but imports `Unity.Netcode`. If it needs network synchronization (RPCs, NetworkVariables), it must inherit `NetworkBehaviour`.

## Key Files
- `Assets/Scripts/Network/NetworkObjectPool.cs`: Central networking pool manager.
- `Assets/Scripts/Abstract/`: Core interfaces (`IDamageable`, `IHealable`).
- `Assets/Scripts/Entities/EntityStatus.cs`: Component for managing entity stats.
