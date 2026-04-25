namespace EntityComponentSystem.Interfaces;

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
