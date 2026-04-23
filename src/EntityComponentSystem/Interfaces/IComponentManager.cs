namespace EntityComponentSystem.Interfaces;

public interface IComponentManager
{
    public void Add<T>(Entity entity, T component) where T : IComponent;
    public void Set<T>(Entity entity, T component) where T : IComponent;
    public void Remove<T>(Entity entity) where T : IComponent;
    public bool TryGet<T>(Entity entity, out T? component) where T : IComponent;
    public T Get<T>(Entity entity) where T : IComponent;
    public bool Has<T>(Entity entity) where T : IComponent;
    public Dictionary<uint, T> GetAll<T>() where T : IComponent;
    public void RemoveAll(Entity entity);
}
