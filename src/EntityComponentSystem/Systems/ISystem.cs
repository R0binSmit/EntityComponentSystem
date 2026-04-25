namespace EntityComponentSystem;

public interface ISystem
{
    int Order => 0;
    void Initialize() { }
    void Shutdown() { }
    void Update(float deltaTime);
}
