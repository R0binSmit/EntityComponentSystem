# EntityComponentSystem

A lightweight, dependency-free ECS framework for .NET — designed as a drop-in submodule for game projects.

## Architecture

An ECS separates concerns into three distinct layers:

| Layer | Role | Framework type |
|---|---|---|
| **Entity** | Unique identifier, no data | `Entity` |
| **Component** | Pure data, no logic | `IComponent` |
| **System** | Logic, no state | `ISystem` |

The framework provides the infrastructure. Application-specific types (e.g. `Position`, `Velocity`, `Health`) are defined in the consuming project as `IComponent` implementations.

---

## File Overview

### `Entity.cs`

A lightweight, generation-tracked identifier. Internally an `(Index, Generation)` pair — the generation increments on destroy, preventing stale references (ABA problem).

```csharp
// Index identifies the slot; Generation identifies the lifetime.
// Two Entity values with the same Index but different Generation are not equal.
Entity a = entityManager.Create(); // Entity(0:0)
entityManager.Destroy(a);
Entity b = entityManager.Create(); // Entity(0:1) — same slot, new generation
a == b // false
```

---

### `Interfaces/IComponent.cs`

Marker interface. Every component type in the consuming project must implement it.

```csharp
public record struct Position(float X, float Y) : IComponent;
public record struct Velocity(float Dx, float Dy) : IComponent;
public record struct Health(int Current, int Max) : IComponent;
```

`record struct` is recommended — value semantics and structural equality with minimal boilerplate.

---

### `Interfaces/IEntityManager.cs`

Manages entity lifetimes. Implemented by `EntityManager`.

```csharp
public interface IEntityManager
{
    Entity Create();
    void Destroy(Entity entity);
    bool IsActive(Entity entity);
    IEnumerable<Entity> All();               // enabled entities only
    bool TryGetByIndex(uint index, out Entity entity);
    void Disable(Entity entity);
    void Enable(Entity entity);
    bool IsEnabled(Entity entity);
}
```

- `Create` recycles destroyed slots (FIFO) before allocating new indices.
- `Destroy` throws if the entity is not active.
- `All()` returns only enabled, active entities — disabled entities are excluded from queries.
- `TryGetByIndex` resolves a raw slot index back to a generation-aware `Entity`.

---

### `Interfaces/IComponentManager.cs`

Manages component storage per entity. Implemented by `ComponentManager`.

```csharp
public interface IComponentManager
{
    void Add<T>(Entity entity, T component) where T : IComponent;
    void Set<T>(Entity entity, T component) where T : IComponent;
    void Remove<T>(Entity entity) where T : IComponent;
    bool TryRemove<T>(Entity entity) where T : IComponent;
    bool TryGet<T>(Entity entity, out T? component) where T : IComponent;
    T Get<T>(Entity entity) where T : IComponent;
    bool Has<T>(Entity entity) where T : IComponent;
    IReadOnlyDictionary<uint, T> GetAll<T>() where T : IComponent;
    void RemoveAll(Entity entity);
    IEnumerable<Type> GetComponentTypes(Entity entity);
    QueryBuilder Query();
    int ComponentVersion { get; }
}
```

- `Add` throws if the entity already has a component of type `T`.
- `Remove` throws if the component is absent; use `TryRemove` for a non-throwing variant.
- `Set` is an upsert — adds or overwrites without throwing.
- `ComponentVersion` is incremented on every mutation and used by `QueryBuilder` for cache invalidation.
- Before destroying an entity, call `RemoveAll` to clean up its components.

---

### `Manager/EntityManager.cs`

Concrete implementation of `IEntityManager`. Tracks active indices, disabled indices, and generation counters.

---

### `Manager/ComponentManager.cs`

Concrete implementation of `IComponentManager`. Stores components in a `Dictionary<Type, Dictionary<uint, T>>` — one sparse map per component type.

---

### `Query/QueryBuilder.cs`

Fluent query builder returned by `componentManager.Query()`.

```csharp
public sealed class QueryBuilder
{
    public QueryBuilder With<T>() where T : IComponent;
    public QueryBuilder Without<T>() where T : IComponent;

    public IEnumerable<Entity> Execute();
    public IEnumerable<(Entity, T1)> Execute<T1>() where T1 : IComponent;
    public IEnumerable<(Entity, T1, T2)> Execute<T1, T2>() ...;
    public IEnumerable<(Entity, T1, T2, T3)> Execute<T1, T2, T3>() ...;
}
```

- Results are cached until `ComponentVersion` changes. **Store `QueryBuilder` as a field** to benefit from caching across frames.
- Internally, the query starts from the smallest matching component store to minimise iterations.

---

### `Systems/ISystem.cs`

Interface for game logic. The consuming project drives the update loop.

```csharp
public interface ISystem
{
    int Order => 0;           // execution order; lower runs first
    void Initialize() { }
    void Shutdown() { }
    void Update(float deltaTime);
}
```

---

### `Events/ComponentEventHub.cs`

Type-safe event dispatcher for component mutations. Handlers are invoked with exception isolation — if multiple handlers throw, all exceptions are collected and re-thrown as a single `AggregateException`.

```csharp
public sealed class ComponentEventHub<T> where T : IComponent
{
    public event Action<Entity, T>? Added;
    public event Action<Entity, T>? Removed;

    public void RaiseAdded(Entity entity, T component);
    public void RaiseRemoved(Entity entity, T component);
}
```

---

## Getting Started

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later

### Installation

```bash
git submodule add https://github.com/R0binSmit/EntityComponentSystem libs/EntityComponentSystem
git submodule update --init --recursive
dotnet sln add libs/EntityComponentSystem/src/EntityComponentSystem/EntityComponentSystem.csproj
```

Reference in your project:

```xml
<ProjectReference Include="..\libs\EntityComponentSystem\src\EntityComponentSystem\EntityComponentSystem.csproj" />
```

---

## Usage Examples

### Define components

```csharp
public record struct Position(float X, float Y) : IComponent;
public record struct Velocity(float Dx, float Dy) : IComponent;
public record struct Health(int Current, int Max) : IComponent;
```

### Entity and component lifecycle

```csharp
var entities   = new EntityManager();
var components = new ComponentManager(entities);

var player = entities.Create();
var enemy  = entities.Create();

components.Add(player, new Position(0, 0));
components.Add(player, new Velocity(1, 0));
components.Add(enemy,  new Position(10, 5));

// Read / update
var pos = components.Get<Position>(player);
components.Set(player, pos with { X = pos.X + 1 });

// Check and remove (throwing)
if (components.Has<Velocity>(player))
    components.Remove<Velocity>(player);

// Non-throwing remove
components.TryRemove<Velocity>(player);

// Try-get pattern
if (components.TryGet<Health>(player, out var hp))
    Console.WriteLine($"HP: {hp.Current}/{hp.Max}");

// Disable / enable (excluded from queries while disabled)
entities.Disable(player);
entities.Enable(player);

// Destroy — clean up components first
components.RemoveAll(enemy);
entities.Destroy(enemy);
```

### Queries

```csharp
// Store as field to benefit from caching across frames
private readonly QueryBuilder _movementQuery = components.Query()
    .With<Position>()
    .With<Velocity>()
    .Without<Health>();

// Plain entity results
foreach (var entity in _movementQuery.Execute())
{
    var pos = components.Get<Position>(entity);
}

// Typed results — components fetched inline
foreach (var (entity, pos, vel) in _movementQuery.Execute<Position, Velocity>())
{
    components.Set(entity, new Position(pos.X + vel.Dx, pos.Y + vel.Dy));
}
```

### Implement a system

```csharp
public class MovementSystem : ISystem
{
    private readonly IComponentManager _components;
    private readonly QueryBuilder _query;

    public int Order => 0;

    public MovementSystem(IEntityManager entities, IComponentManager components)
    {
        _components = components;
        _query = components.Query().With<Position>().With<Velocity>();
    }

    public void Update(float deltaTime)
    {
        foreach (var (entity, pos, vel) in _query.Execute<Position, Velocity>())
            _components.Set(entity, new Position(
                pos.X + vel.Dx * deltaTime,
                pos.Y + vel.Dy * deltaTime));
    }
}
```

### Event hub

```csharp
var healthEvents = new ComponentEventHub<Health>();

healthEvents.Added   += (entity, hp) => Console.WriteLine($"{entity} gained Health({hp.Max})");
healthEvents.Removed += (entity, hp) => Console.WriteLine($"{entity} lost Health");

// Wire into your own facade:
var player = entities.Create();
var hp = new Health(100, 100);
components.Add(player, hp);
healthEvents.RaiseAdded(player, hp);
```

---

## Design Decisions

| Decision | Rationale |
|---|---|
| One component type per entity | Keeps storage simple and queries predictable |
| Dictionary-based storage | Prioritises correctness over raw cache performance |
| No thread safety | Game loops are single-threaded; locking adds overhead with no benefit |
| No World / facade | Consuming projects wire the managers together themselves |
| No built-in app concepts | Labels, names, hierarchy are `IComponent` implementations in the consuming project |
