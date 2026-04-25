using EntityComponentSystem.Interfaces;

namespace EntityComponentSystem.Manager;

public class ComponentManager : IComponentManager
{
    private readonly IEntityManager _entityManager;
    private readonly Dictionary<Type, object> _stores = new();
    private int _componentVersion = 0;

    public int ComponentVersion => _componentVersion;

    public ComponentManager(IEntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    private Dictionary<uint, T> GetStore<T>() where T : IComponent
    {
        if (!_stores.TryGetValue(typeof(T), out var store))
        {
            store = new Dictionary<uint, T>();
            _stores[typeof(T)] = store;
        }
        return (Dictionary<uint, T>)store;
    }

    private void AssertActive(Entity entity)
    {
        if (!_entityManager.IsActive(entity))
            throw new InvalidOperationException($"Entity {entity} is not active.");
    }

    public void Add<T>(Entity entity, T component) where T : IComponent
    {
        AssertActive(entity);
        if (!GetStore<T>().TryAdd(entity.Index, component))
            throw new InvalidOperationException(
                $"Entity {entity} already has a component of type {typeof(T).Name}.");
        _componentVersion++;
    }

    public void Set<T>(Entity entity, T component) where T : IComponent
    {
        AssertActive(entity);
        GetStore<T>()[entity.Index] = component;
        _componentVersion++;
    }

    public void Remove<T>(Entity entity) where T : IComponent
    {
        if (!GetStore<T>().Remove(entity.Index))
            throw new InvalidOperationException(
                $"Entity {entity} does not have a component of type {typeof(T).Name}.");
        _componentVersion++;
    }

    public bool TryRemove<T>(Entity entity) where T : IComponent
    {
        if (!GetStore<T>().Remove(entity.Index))
            return false;
        _componentVersion++;
        return true;
    }

    public bool TryGet<T>(Entity entity, out T? component) where T : IComponent
    {
        return GetStore<T>().TryGetValue(entity.Index, out component);
    }

    public T Get<T>(Entity entity) where T : IComponent
    {
        AssertActive(entity);
        if (!GetStore<T>().TryGetValue(entity.Index, out var component))
            throw new InvalidOperationException(
                $"Entity {entity} does not have a component of type {typeof(T).Name}.");
        return component;
    }

    public bool Has<T>(Entity entity) where T : IComponent
    {
        return GetStore<T>().ContainsKey(entity.Index);
    }

    public IReadOnlyDictionary<uint, T> GetAll<T>() where T : IComponent
    {
        return GetStore<T>();
    }

    public void RemoveAll(Entity entity)
    {
        bool changed = false;
        foreach (var store in _stores.Values)
        {
            if (store is System.Collections.IDictionary dict && dict.Contains(entity.Index))
            {
                dict.Remove(entity.Index);
                changed = true;
            }
        }
        if (changed) _componentVersion++;
    }

    public IEnumerable<Type> GetComponentTypes(Entity entity)
    {
        return _stores
            .Where(kv => ((System.Collections.IDictionary)kv.Value).Contains(entity.Index))
            .Select(kv => kv.Key);
    }

    public QueryBuilder Query() => new QueryBuilder(_entityManager, this);
}
