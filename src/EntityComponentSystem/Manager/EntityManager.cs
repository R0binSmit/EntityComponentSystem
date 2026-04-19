namespace EntityComponentSystem.Manager;

public class EntityManager
{
    private uint _nextId = 0;
    private readonly Queue<uint> _recycled = new();
    private readonly HashSet<uint> _activeEntities = new();

    public Entity Create()
    {
        uint id = _recycled.Count > 0 ? _recycled.Dequeue() : _nextId++;
        _activeEntities.Add(id);
        return new Entity(id);
    }

    public void Destroy(Entity entity)
    {
        _activeEntities.Remove(entity.Id);
        _recycled.Enqueue(entity.Id);
    }

    public bool IsActive(Entity entity) => _activeEntities.Contains(entity.Id);
    public IEnumerable<Entity> All => _activeEntities.Select(id => new Entity(id));
}
