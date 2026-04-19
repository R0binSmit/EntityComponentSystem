namespace EntityComponentSystem;

public readonly struct Entity
{
    public readonly uint Id;
    public Entity(uint id) => Id = id;
    public static implicit operator uint(Entity e) => e.Id;
    public override string ToString() => $"Entity({Id})";
}