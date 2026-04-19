using EntityComponentSystem.Interfaces;
using System.Collections;

namespace EntityComponentSystem.Manager;

public class ComponentManager
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

    public void Add<T>(Entity entity, T component) where T : IComponent => GetStore<T>()[entity.Id] = component;
    public void Remove<T>(Entity entity) where T : IComponent => GetStore<T>().Remove(entity.Id);
    public void TryGet<T>(Entity entity, out T? component) where T : IComponent => GetStore<T>().TryGetValue(entity.Id, out component);
    public T Get<T>(Entity entity) where T : IComponent => GetStore<T>()[entity.Id];
    public bool Has<T>(Entity entity) where T : IComponent => GetStore<T>().ContainsKey(entity.Id);
    public IEnumerable<(Entity, T)> GetAll<T>() where T : IComponent => GetStore<T>().Select(kv => (new Entity(kv.Key), kv.Value));

    public void RemoveAll(Entity entity)
    {
        foreach (var store in _stores.Values)
        {
            var dict = store as IDictionary;
            dict?.Remove(entity.Id);
        }
    }
}
