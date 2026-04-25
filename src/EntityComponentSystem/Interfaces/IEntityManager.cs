namespace EntityComponentSystem.Interfaces;

public interface IEntityManager
{
    Entity Create();
    void Destroy(Entity entity);
    bool IsActive(Entity entity);
    IEnumerable<Entity> All();
    bool TryGetByIndex(uint index, out Entity entity);
    void Disable(Entity entity);
    void Enable(Entity entity);
    bool IsEnabled(Entity entity);
}
