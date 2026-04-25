using EntityComponentSystem.Interfaces;

namespace EntityComponentSystem;

public sealed class ComponentEventHub<T> where T : IComponent
{
    public event Action<Entity, T>? Added;
    public event Action<Entity, T>? Removed;

    public void RaiseAdded(Entity entity, T component) => RaiseSafe(Added, entity, component);
    public void RaiseRemoved(Entity entity, T component) => RaiseSafe(Removed, entity, component);

    private static void RaiseSafe(Action<Entity, T>? @event, Entity entity, T component)
    {
        if (@event == null) return;
        List<Exception>? errors = null;
        foreach (var handler in @event.GetInvocationList().Cast<Action<Entity, T>>())
        {
            try { handler(entity, component); }
            catch (Exception ex) { (errors ??= new()).Add(ex); }
        }
        if (errors != null) throw new AggregateException(errors);
    }
}
