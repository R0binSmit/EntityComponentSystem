namespace EntityComponentSystem.Interfaces;

public interface IEntityManager
{
    public Entity Create();
    public void Destroy(Entity entity);
    public bool IsActive(Entity entity);
    public IEnumerable<Entity> All();
}
