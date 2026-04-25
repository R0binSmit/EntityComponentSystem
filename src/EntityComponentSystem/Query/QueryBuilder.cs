using EntityComponentSystem.Interfaces;

namespace EntityComponentSystem;

public sealed class QueryBuilder
{
    private readonly IEntityManager _entityManager;
    private readonly IComponentManager _componentManager;

    private readonly List<(Func<int> Count, Func<IEnumerable<uint>> Indices, Func<Entity, bool> Check)> _withFilters = new();
    private readonly List<Func<Entity, bool>> _withoutFilters = new();

    private int _cachedVersion = -1;
    private IReadOnlyList<Entity> _cachedResults = Array.Empty<Entity>();

    internal QueryBuilder(IEntityManager entityManager, IComponentManager componentManager)
    {
        _entityManager = entityManager;
        _componentManager = componentManager;
    }

    public QueryBuilder With<T>() where T : IComponent
    {
        _withFilters.Add((
            Count: () => _componentManager.GetAll<T>().Count,
            Indices: () => _componentManager.GetAll<T>().Keys,
            Check: entity => _componentManager.Has<T>(entity)
        ));
        return this;
    }

    public QueryBuilder Without<T>() where T : IComponent
    {
        _withoutFilters.Add(entity => !_componentManager.Has<T>(entity));
        return this;
    }

    /// <summary>
    /// Returns matching entities. Results are cached until any component is added or removed.
    /// Store this QueryBuilder instance as a field to benefit from caching across frames.
    /// </summary>
    public IEnumerable<Entity> Execute()
    {
        int version = _componentManager.ComponentVersion;
        if (_cachedVersion == version)
            return _cachedResults;

        _cachedResults = RunQuery().ToList();
        _cachedVersion = version;
        return _cachedResults;
    }

    public IEnumerable<(Entity Entity, T1 C1)> Execute<T1>()
        where T1 : IComponent
        => Execute().Select(e => (e, _componentManager.Get<T1>(e)));

    public IEnumerable<(Entity Entity, T1 C1, T2 C2)> Execute<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
        => Execute().Select(e => (e, _componentManager.Get<T1>(e), _componentManager.Get<T2>(e)));

    public IEnumerable<(Entity Entity, T1 C1, T2 C2, T3 C3)> Execute<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        => Execute().Select(e => (e, _componentManager.Get<T1>(e), _componentManager.Get<T2>(e), _componentManager.Get<T3>(e)));

    private IEnumerable<Entity> RunQuery()
    {
        IEnumerable<Entity> candidates;
        IEnumerable<Func<Entity, bool>> remainingChecks;

        if (_withFilters.Count > 0)
        {
            int seedIndex = 0;
            int minCount = _withFilters[0].Count();
            for (int i = 1; i < _withFilters.Count; i++)
            {
                int count = _withFilters[i].Count();
                if (count < minCount) { minCount = count; seedIndex = i; }
            }

            candidates = _withFilters[seedIndex].Indices()
                .Select(index => _entityManager.TryGetByIndex(index, out var e) ? (Entity?)e : null)
                .Where(e => e.HasValue && _entityManager.IsEnabled(e!.Value))
                .Select(e => e!.Value);

            remainingChecks = _withFilters
                .Select((f, i) => (f, i))
                .Where(t => t.i != seedIndex)
                .Select(t => t.f.Check)
                .Concat(_withoutFilters);
        }
        else
        {
            candidates = _entityManager.All();
            remainingChecks = _withoutFilters;
        }

        var checks = remainingChecks.ToList();
        return checks.Count == 0
            ? candidates
            : candidates.Where(entity => checks.All(f => f(entity)));
    }
}
