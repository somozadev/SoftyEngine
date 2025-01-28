using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Softine;

public class GameEngine
{
    private RenderWindow window;
    private SceneManager sceneManager = new SceneManager();
    private Physics physicsEngine;
    private bool isRunning = false;

    private const float physicsDeltaTime = 1.0f / 60.0f;
    private float physicsAccumulator = 0.0f;
    private Clock clock;

    public GameEngine(uint width, uint height, string title)
    {
        window = new RenderWindow(new VideoMode(width, height), title);
        window.Closed += (sender, e) => window.Close();
        physicsEngine = new Physics();
        clock = new Clock();
    }


    public void Run()
    {
        isRunning = true;
        var lastFrameTime = 0.0f;

        while (isRunning && window.IsOpen)
        {
            HandleEvents();
            var deltaTime = clock.Restart().AsSeconds();
            lastFrameTime += deltaTime;

            physicsAccumulator += deltaTime;
            while (physicsAccumulator >= physicsDeltaTime)
            {
                FixedUpdate(physicsDeltaTime);
                physicsAccumulator -= physicsDeltaTime;
            }

            Update(deltaTime);
            Render();
        }
    }

    private void HandleEvents()
    {
        window.DispatchEvents();
    }

    private void FixedUpdate(float deltaTime)
    {
        physicsEngine.Tick(deltaTime);
        sceneManager.FixedUpdate(deltaTime);
    }

    private void Update(float deltaTime)
    {
        sceneManager.Update(deltaTime);
        physicsEngine?.Tick(deltaTime);
    }

    private void Render()
    {
        window.Clear();
        // currentScene?.Render(window);
        window.Display();
    }
}