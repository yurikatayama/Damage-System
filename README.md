# DamageSystem Package Guide

A modular, decoupled damage and resource system for Unity. Drop it into any project and wire it up in minutes.

---

## Namespace

All classes live under `GodMachine.DamageSystem`. Add this at the top of any file that uses the package:

```csharp
using GodMachine.DamageSystem;
```

---

## Package Contents

| File | Type | Role |
|---|---|---|
| `IDamageable` | Interface | Entry point for all incoming damage |
| `IDefensable` | Interface | Exposes defense state to the damage pipeline |
| `DamageData` | Struct | Carries raw damage amount and source |
| `DamageResult` | Struct | Carries resolved damage after mitigation |
| `DamageHitState` | Enum | Describes the outcome of a hit |
| `DamageDealer` | MonoBehaviour | Sends damage on trigger overlap |
| `DamageReceiver` | MonoBehaviour | Orchestrates the full damage resolution |
| `DamageResolver` | Static Class | Pure damage math, no dependencies |
| `Health` | MonoBehaviour | HP store and events |
| `Stamina` | MonoBehaviour | Stamina store, regen, and brief respite |
| `Invulnerability` | MonoBehaviour | Timed invulnerability state |

---

## Class Reference

### IDamageable

The single entry point for all incoming damage. Any entity that can be damaged must have a component implementing this interface on its GameObject. External sources never bypass it.

```csharp
public interface IDamageable
{
    void TakeDamage(DamageData damage);
}
```

`DamageReceiver` is the default implementation provided by this package. You should not need to implement this yourself unless you have a very specific use case that bypasses the standard pipeline.

---

### IDefensable

Exposes the defense state of an entity to the damage pipeline. `DamageReceiver` looks for this interface on the same GameObject and reads from it during damage resolution.

```csharp
public interface IDefensable
{
    bool IsParrying { get; }
    bool IsBlocking { get; }
    float DamageReduction { get; }
}
```

Implement this on your defense component (e.g. `PlayerDefense`, `BossDefense`) to plug into the pipeline. Keep this component focused on state and mechanics only. The actual mitigation math lives in `DamageResolver`, which reads from your `IDefensable` implementation automatically.

`IDefensable` is designed to be extended. As your combat system grows you can add properties like armor rating, elemental resistance, or damage reflection percentage. Any new property added here is immediately available to `DamageResolver`, where you can incorporate it into the mitigation formula.

Your `IDefensable` component is also the right place to handle side effects triggered by a hit outcome. Subscribe to `DamageReceiver.OnDamageResolved` and react based on `DamageHitState`:

```csharp
private void Start()
{
    GetComponent<DamageReceiver>().OnDamageResolved += HandleDamageResolved;
}

private void HandleDamageResolved(DamageResult result)
{
    switch (result.HitState)
    {
        case DamageHitState.Parried:
            // recover stamina, trigger parry animation, fire stagger event, etc.
            break;
        case DamageHitState.Blocked:
            // consume stamina, play block sound, etc.
            break;
    }
}
```

---

### DamageData

A readonly struct that carries the raw incoming damage before any mitigation is applied.

```csharp
public readonly struct DamageData
{
    public readonly int Amount;
    public readonly Component Source;
}
```

`Amount` is the raw damage value. `Source` is the Component that initiated the damage, useful for knockback direction, hit reactions, or logging.

`DamageData` is designed to be extended. As your project grows you can add properties like damage type (physical, magical, elemental), origin position for knockback calculations, or any other context your pipeline needs. `DamageResolver` and `DamageReceiver` both receive it, so any new properties you add are immediately available throughout the full resolution pipeline.

---

### DamageResult

A readonly struct that carries the fully resolved damage after mitigation, invulnerability checks, and defense state evaluation.

```csharp
public readonly struct DamageResult
{
    public readonly int FinalDamage;
    public readonly DamageHitState HitState;
    public readonly Component Source;
}
```

Returned via `DamageReceiver.OnDamageResolved`. Use it to drive hit reactions, UI feedback, audio, and defense side effects.

`DamageResult` is designed to be extended. As your project grows you can add properties like hit position, damage dealt before mitigation, or status effects applied by the hit. Everything that subscribes to `OnDamageResolved` will have access to the new data immediately.

---

### DamageHitState

An enum describing the outcome of a damage resolution.

```csharp
public enum DamageHitState
{
    Damaged,      // Full or unmitigated damage was applied
    Blocked,      // Hit was blocked, damage was reduced
    Parried,      // Hit was parried, damage was voided
    Invulnerable  // Entity was invulnerable, no damage applied
}
```

`DamageHitState` is designed to be extended. Add new states as your combat system grows, such as `Reflected`, `Absorbed`, or `StatusApplied`. Any component subscribed to `DamageReceiver.OnDamageResolved` can react to new states without changes to the core pipeline.

---

### DamageDealer

A MonoBehaviour that sends damage when its collider overlaps a target with `IDamageable`. Attach it to any hitbox object.

```csharp
[SerializeField] private int damage = 10;
[SerializeField] private bool destroyOnHit = false;
```

Set `damage` in the inspector for simple use cases. For more complex projects, use `SetDamage(int value)` to drive the damage value externally from your own attack or ability system:

```csharp
_damageDealer.SetDamage(calculatedDamage);
```

Enable and disable the GameObject to control when the hitbox is active. Cycling the GameObject off and on between swings is also the recommended way to handle multihit prevention. `DamageDealer` is intentionally dumb. It holds a damage value, finds `IDamageable` on the target, and calls `TakeDamage()`. Everything else is handled downstream.

---

### DamageReceiver

A MonoBehaviour that implements `IDamageable` and orchestrates the full damage resolution pipeline. Add this to every entity that can take damage.

On each hit it:

1. Checks `Invulnerability` on the same GameObject
2. Reads `IDefensable` state if present
3. Delegates math to `DamageResolver`
4. Fires `OnDamageResolved` with the result
5. Calls `Health.ApplyDamage()` with the final damage

```csharp
public event System.Action<DamageResult> OnDamageResolved;
```

Subscribe to `OnDamageResolved` to react to any hit outcome from anywhere in your project.

---

### DamageResolver

A static class that contains the damage mitigation math. It has no MonoBehaviour, no dependencies, and no side effects.

```csharp
public static int Resolve(DamageData damage, IDefensable defense)
```

Pass `null` as the defense argument if there is no defense component. It returns the raw damage amount unchanged. You can call this directly if you need damage calculations outside the main pipeline.

Mitigation rules:
- If parrying: returns 0
- If blocking: returns `damage * (1 - DamageReduction / 100)`
- Otherwise: returns raw damage amount

---

### Health

A MonoBehaviour that stores and manages HP. It fires events when values change and handles death. It knows nothing about damage sources, defense, or mitigation. It is only ever written to by `DamageReceiver` internally.

```csharp
public int CurrentHP { get; }
public int MaxHP { get; }
public bool IsDead { get; }

public event System.Action<int, int> OnHealthChanged;
public event System.Action<int> OnDamageTaken;
public event System.Action OnDeath;
```

Initialize max HP from your stats system in `Start()`:

```csharp
private void Start()
{
    GetComponent<Health>().SetMaxHP(myStats.MaxHP);
}
```

The serialized `_maxHP` field acts as a default for entities with no stats system, such as destructible objects.

Health regen is available but disabled by default. Enable it by calling `SetRegen()` with a value greater than 0:

```csharp
GetComponent<Health>().SetRegen(5f); // 5 HP per second
```

Passing 0 disables regen. Regen is a simple per-second tick with no delay or respite logic. If you need more complex regen behavior (delay after damage, respite windows), drive it externally using `Heal()` from your own system.

Do not call `ApplyDamage()` from outside the GameObject. All external damage must enter through `DamageReceiver`.

---

### Stamina

A MonoBehaviour that manages a stamina resource with automatic regeneration and brief respite. It has no dependencies on any stats system and must be initialized externally.

```csharp
public float Current { get; }
public float Max { get; }
public bool IsExhausted { get; }

public event System.Action<float, float> OnStaminaChanged;
```

Initialize from your stats system in `Start()`:

```csharp
private void Start()
{
    Stamina stamina = GetComponent<Stamina>();
    stamina.SetMax(100f);
    stamina.SetRegen(15f);      // stamina per second
    stamina.SetRespite(0.5f);   // seconds before regen resumes after consumption
}
```

Consuming stamina:

```csharp
stamina.Consume(25f);           // instant cost
stamina.ConsumeOverTime(10f);   // per-second drain, call in Update()
```

Recovering stamina:

```csharp
stamina.Recover(20f);
```

Pausing regen manually (e.g. while blocking):

```csharp
stamina.SetRegenPaused(true);
stamina.SetRegenPaused(false);
```

Stamina is a soft gate. Actions are never hard-blocked by this class. If a cost exceeds the current stamina, it consumes what is available, clamps to 0, and triggers brief respite. Use `IsExhausted` in your own logic to apply penalties if needed.

Brief respite is triggered automatically on any consumption. Regen resumes after the respite duration expires. Regen pause overrides respite and blocks regen indefinitely until released.

---

### Invulnerability

A MonoBehaviour that manages timed invulnerability. `DamageReceiver` checks this component automatically. Grant invulnerability from anywhere:

```csharp
GetComponent<Invulnerability>().Grant(0.5f); // 0.5 seconds of invulnerability
```

Calling `Grant()` while already invulnerable extends the duration if the new value is longer. It never shortens an active window.

---

## Setup Guide

### Entity with no defense (destructible object, simple enemy)

1. Add `Health`
2. Add `DamageReceiver`
3. Done

```csharp
GetComponent<Health>().OnDeath += HandleDeath;
GetComponent<DamageReceiver>().OnDamageResolved += HandleHit;
```

### Entity with defense (player, boss)

1. Add `Health`, `DamageReceiver`
2. Implement `IDefensable` on your defense component:

```csharp
public class MyDefense : MonoBehaviour, IDefensable
{
    public bool IsParrying { get; private set; }
    public bool IsBlocking { get; private set; }
    public float DamageReduction => 50f;

    private void Start()
    {
        GetComponent<DamageReceiver>().OnDamageResolved += HandleDamageResolved;
    }

    private void HandleDamageResolved(DamageResult result)
    {
        if (result.HitState == DamageHitState.Parried)
        {
            // handle parry side effects
        }
    }
}
```

3. Add your defense component to the same GameObject as `DamageReceiver`

### Dealing damage

1. Create a child GameObject with a Collider set to Is Trigger
2. Add `DamageDealer` and set the damage value
3. Enable and disable the GameObject to activate and deactivate the hitbox

---

## Quick Setup

### Player (or any entity that takes damage and can defend)

1. Add `Health` to your player GameObject
2. Add `DamageReceiver` to your player GameObject
3. Add `Invulnerability` to your player GameObject (optional, required for i-frames)
4. Add `Stamina` to your player GameObject (optional, required for stamina-gated actions)
5. Create a defense component that implements `IDefensable` and add it to your player GameObject
6. In your defense component `Start()`, call `GetComponent<Health>().SetMaxHP(yourMaxHP)`
7. In your defense component `Start()`, subscribe to `GetComponent<DamageReceiver>().OnDamageResolved`
8. Handle stamina costs and parry events inside that subscription

### Enemy (or any entity that takes damage with no defense)

1. Add `Health` to your enemy GameObject
2. Add `DamageReceiver` to your enemy GameObject
3. Subscribe to `GetComponent<Health>().OnDeath` to handle death behavior

### Hitbox

1. Create a child GameObject on your attacker
2. Add a Collider and set it to Is Trigger
3. Add `DamageDealer` to that child GameObject
4. Set the damage value in the inspector, or call `SetDamage(value)` before activating
5. Enable and disable that child GameObject to activate and deactivate the hitbox

---

## Rules

- Never call `Health.ApplyDamage()` from outside the GameObject. All external damage must go through `DamageReceiver`.
- One `DamageReceiver` per entity. It is the single entry point.
- `IDefensable` exposes state only. Keep mitigation math out of your defense components.
- Initialize `Health` and `Stamina` from your stats system in `Start()`, after all `Awake()` calls have completed.
- `DamageDealer` is a dumb sender. Hit filtering (multihit prevention, target validation) is the responsibility of whatever controls the hitbox activation.
