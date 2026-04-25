using EntityComponentSystem.Interfaces;

namespace EntityComponentSystem.Manager;

public class EntityManager : IEntityManager
{
    private uint _nextIndex = 0;
    private readonly Queue<uint> _recycled = new();
    private readonly HashSet<uint> _activeIndices = new();
    private readonly HashSet<uint> _disabledIndices = new();
    private readonly Dictionary<uint, uint> _generations = new();

    public Entity Create()
    {
        uint index = _recycled.Count > 0 ? _recycled.Dequeue() : _nextIndex++;
        if (!_generations.ContainsKey(index))
            _generations[index] = 0;
        _activeIndices.Add(index);
        return new Entity(index, _generations[index]);
    }

    public void Destroy(Entity entity)
    {
        if (!IsActive(entity))
            throw new InvalidOperationException($"Cannot destroy {entity}: entity is not active.");
        _disabledIndices.Remove(entity.Index);
        _activeIndices.Remove(entity.Index);
        _generations[entity.Index] = entity.Generation + 1;
        _recycled.Enqueue(entity.Index);
    }

    public bool IsActive(Entity entity) =>
        _activeIndices.Contains(entity.Index)
        && _generations.TryGetValue(entity.Index, out var gen)
        && gen == entity.Generation;

    public IEnumerable<Entity> All() =>
        _activeIndices
            .Where(index => !_disabledIndices.Contains(index))
            .Select(index => new Entity(index, _generations[index]));

    public bool TryGetByIndex(uint index, out Entity entity)
    {
        if (_activeIndices.Contains(index) && _generations.TryGetValue(index, out var gen))
        {
            entity = new Entity(index, gen);
            return true;
        }
        entity = default;
        return false;
    }

    public void Disable(Entity entity)
    {
        if (!IsActive(entity))
            throw new InvalidOperationException($"Cannot disable {entity}: entity is not active.");
        _disabledIndices.Add(entity.Index);
    }

    public void Enable(Entity entity)
    {
        if (!IsActive(entity))
            throw new InvalidOperationException($"Cannot enable {entity}: entity is not active.");
        _disabledIndices.Remove(entity.Index);
    }

    public bool IsEnabled(Entity entity) =>
        IsActive(entity) && !_disabledIndices.Contains(entity.Index);

}
