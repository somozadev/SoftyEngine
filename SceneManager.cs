using SoftyEngine.ECS;

namespace Softine;

public class SceneManager()
{
    private Scene _currentScene;
    private readonly Dictionary<string, Scene> _scenes = new();
    private List<ISystem> systems;
    private List<IPhysicsSystem> physicsSystems;
    
    
    public void AddScene(Scene scene)
    {
        _scenes[scene.Name] = scene;
        _currentScene ??= scene;
    }

    public void SwitchScene(string name)
    {
        if (_scenes.TryGetValue(name, out var scene))
        {
            _currentScene = scene;
        }
        else
        {
            throw new Exception($"Scene '{name}' does not exist");
        }
    }

    public void Update(float deltaTime)
    {
        _currentScene?.Update(deltaTime);
    }

    public void FixedUpdate(float fixedDeltaTime)
    {
        _currentScene?.FixedUpdate(fixedDeltaTime);
    }
}