using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Softine.Utils;
using SoftyEngine.ECS;
using Transform = SoftyEngine.ECS.Transform;

namespace Softine
{
    static class Program
    {
        static void OnClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        static void Main()
        {
            RenderWindow window = InitWindow();
            Clock clock = new Clock();

            var sceneManager = new SceneManager();
            Scene scene = new Scene("FirstScene");
            // Entity entity = scene.Instantiate(new Entity("Body"));
            // Entity entity2 = scene.Instantiate(new Entity("Body2"));
            // Entity entity3 = scene.Instantiate(new Entity("Body3"));

            // entity.AddComponent(new Transform(new Vector2f(GameState.windowWidth / 2, GameState.windowHeight / 2),
            //     100));
            // entity.AddComponent(new SoftBodyComponent(SoftBodyType.SQUARE));
            // entity.AddComponent(new ComplexRendererComponent());
            // entity2.AddComponent(
            //     new Transform(new Vector2f(GameState.windowWidth / 5, GameState.windowHeight / 2), 100));
            // entity2.AddComponent(new SoftBodyComponent(SoftBodyType.CIRCLE));
            // entity2.AddComponent(new ComplexRendererComponent());
            // entity3.AddComponent(
            //     new Transform(new Vector2f(GameState.windowWidth / 1.5f, GameState.windowHeight / 2), 100));
            // entity3.AddComponent(new SoftBodyComponent(SoftBodyType.POLY));
            // entity3.AddComponent(new ComplexRendererComponent());

            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new RenderSystem(window));
            scene.AddSystem(new ComplexRenderSystem(window));
            scene.AddPhysicsSystem(new PhysicsSystem());
            scene.AddPhysicsSystem(new CollisionSystem());
            sceneManager.AddScene(scene);
            Font font = new Font("Fonts/AldotheApache.ttf");
            Text fpsText = new Text("FPS: 0", font, 24)
            {
                FillColor = Color.White,
                Position = new Vector2f(10, 10)
            };
            window.MouseButtonPressed += (sender, e) =>
            {
                Vector2f mousePosition = new Vector2f(e.X, e.Y);
                if (e.Button == Mouse.Button.Left)
                {
                    CreateSoftBodyEntity(scene, mousePosition, 100f, SoftBodyType.SQUARE);
                }
                if (e.Button == Mouse.Button.Right)
                {
                    CreateSoftBodyEntity(scene, mousePosition, 100f, SoftBodyType.CIRCLE);
                }
            };
            var fixedDeltaTime = 1.0f / 120.0f;
            float accumulatedTime = 0.0f;

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(GameState.windowColor);


                float deltaTime = clock.Restart().AsSeconds();
                accumulatedTime += deltaTime;
                float fps = 1.0f / deltaTime;

                sceneManager.Update(deltaTime);
                while (accumulatedTime >= fixedDeltaTime)
                {
                    sceneManager.FixedUpdate(fixedDeltaTime);
                    accumulatedTime -= fixedDeltaTime;
                }

                fpsText.DisplayedString = $"FPS: {fps:F1}   \n    Entities: {scene.GetEntitiesAmount()}";
                window.Draw(fpsText);

                window.Display();
            }
        }

        private static RenderWindow InitWindow()
        {
            RenderWindow window =
                new RenderWindow(new VideoMode((uint)GameState.windowWidth, (uint)GameState.windowHeight),
                    "SoftyEngine", Styles.Resize | Styles.Close);
            window.Closed += OnClose;
            window.Resized += (sender, e) =>
            {
                View view = new View(new FloatRect(0, 0, window.Size.X, window.Size.Y));
                window.SetView(view);
            };
            window.SetVerticalSyncEnabled(false);
            window.SetFramerateLimit(0);

            return window;
        }

        private static void CreateSoftBodyEntity(Scene scene, Vector2f position, float scale, SoftBodyType softBodyType)
        {
            Entity entity = scene.Instantiate(new Entity($"Entity_{Guid.NewGuid()}"));
            entity.AddComponent(new Transform(position, scale));
            entity.AddComponent(new SoftBodyComponent(softBodyType));
            entity.AddComponent(new ComplexRendererComponent());
        }


        // private static void TestForces(SoftBodyObject test)
        // {
        //     var force = Vector2.ZeroVector;
        //     if (Keyboard.IsKeyPressed(Keyboard.Key.W))
        //     {
        //         force.Y = -100f;
        //     }
        //
        //     if (Keyboard.IsKeyPressed(Keyboard.Key.A))
        //     {
        //         force.X = -100f;
        //     }
        //
        //     if (Keyboard.IsKeyPressed(Keyboard.Key.S))
        //     {
        //         force.Y = 100f;
        //     }
        //
        //     if (Keyboard.IsKeyPressed(Keyboard.Key.D))
        //     {
        //         force.X = 100f;
        //     }
        //
        //     if (Keyboard.IsKeyPressed(Keyboard.Key.LShift))
        //     {
        //         if (Keyboard.IsKeyPressed(Keyboard.Key.Up))
        //         {
        //             foreach (var spring in test.Springs)
        //             {
        //                 spring.Damping -= spring.Damping * 0.2f;
        //             }
        //         }
        //
        //         if (Keyboard.IsKeyPressed(Keyboard.Key.Down))
        //         {
        //             foreach (var spring in test.Springs)
        //             {
        //                 spring.Damping += spring.Damping * 0.2f;
        //             }
        //         }
        //     }
        //
        //     test.ApplyForce(force);
        // }
    }
}