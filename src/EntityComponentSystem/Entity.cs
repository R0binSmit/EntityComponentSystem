namespace EntityComponentSystem;

public readonly struct Entity : IEquatable<Entity>
{
    public readonly uint Index;
    public readonly uint Generation;

    public Entity(uint index, uint generation)
    {
        Index = index;
        Generation = generation;
    }

    public static implicit operator uint(Entity e) => e.Index;

    public bool Equals(Entity other) => Index == other.Index && Generation == other.Generation;
    public override bool Equals(object? obj) => obj is Entity e && Equals(e);
    public override int GetHashCode() => HashCode.Combine(Index, Generation);
    public static bool operator ==(Entity a, Entity b) => a.Equals(b);
    public static bool operator !=(Entity a, Entity b) => !a.Equals(b);
    public override string ToString() => $"Entity({Index}:{Generation})";
}
