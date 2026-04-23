using EntityComponentSystem.Interfaces;

namespace EntityComponentSystem.Manager;

public class ComponentManager : IComponentManager
{
    private readonly Dictionary<Type, object> _stores = new();

    private Dictionary<uint, T> GetStore<T>() where T : IComponent
    {
        if (!_stores.TryGetValue(typeof(T), out var store))
        {
            store = new Dictionary<uint, T>();
            _stores[typeof(T)] = store;
        }
        return (Dictionary<uint, T>)store;
    }

    public void Add<T>(Entity entity, T component) where T : IComponent
    {
        var store = GetStore<T>();
        if (!store.TryAdd(entity.Id, component))
            throw new InvalidOperationException(
                $"Entity {entity.Id} already has a component of type {typeof(T).Name}.");
    }

    public void Set<T>(Entity entity, T component) where T : IComponent
    {
        GetStore<T>()[entity.Id] = component;
    }

    public void Remove<T>(Entity entity) where T : IComponent
    {
        GetStore<T>().Remove(entity.Id);
    }

    public bool TryGet<T>(Entity entity, out T? component) where T : IComponent
    {
        return GetStore<T>().TryGetValue(entity.Id, out component);
    }

    public T Get<T>(Entity entity) where T : IComponent
    {
        return GetStore<T>()[entity.Id];
    }
    public bool Has<T>(Entity entity) where T : IComponent
    {
        return GetStore<T>().ContainsKey(entity.Id);
    }
    public Dictionary<uint, T> GetAll<T>() where T : IComponent
    {
        return GetStore<T>();
    } 

    public void RemoveAll(Entity entity)
    {
        foreach (var store in _stores.Values)
        {
            if (store is System.Collections.IDictionary dict)
            {       
                dict.Remove(entity.Id);
            }
        }           
    }
}
