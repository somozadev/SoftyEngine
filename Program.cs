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
            ImGuiManager imgui = new ImGuiManager(window);
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
            window.SetActive(true);
            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new RenderSystem(window));
            scene.AddSystem(new SoftRenderSystem(window));
            scene.AddPhysicsSystem(new PhysicsSystem());
            scene.AddPhysicsSystem(new CollisionSystem());
            scene.AddPhysicsSystem(new RigidCollisionSystem());
            sceneManager.AddScene(scene);
            Font font = new Font("Fonts/AldotheApache.ttf");
            Text fpsText = new Text("FPS: 0", font, 24)
            {
                FillColor = Color.White,
                Position = new Vector2f(10, 10)
            };

            bool leftMouseHeld = false;
            bool rightMouseHeld = false;
            float creationCooldown = 0.6f;
            float timeSinceLastCreation = 0f;
            RigidBodyType selectedRigidType = RigidBodyType.CIRCLE;
            SoftBodyType selectedSoftType = SoftBodyType.CIRCLE;
            bool rigidbodyMode = false;
            window.MouseButtonPressed += (sender, e) =>
            {
                if (e.Button == Mouse.Button.Left)
                    leftMouseHeld = true;

                if (e.Button == Mouse.Button.Right)
                    rightMouseHeld = true;
            };
            window.KeyPressed += (sender, args) =>
            {
                if (args.Code == Keyboard.Key.Space)
                {
                    rigidbodyMode = !rigidbodyMode;
                    if(rigidbodyMode)
                        creationCooldown = 0.05f;
                    else
                        creationCooldown = 0.6f;

                }

                if (args.Control)
                {
                    if (rigidbodyMode)
                    {
                        if (selectedRigidType == (RigidBodyType)3)
                            selectedRigidType = (RigidBodyType)0;
                        else
                            selectedRigidType += 1;
                    }
                    else
                    {
                        if (selectedSoftType == (SoftBodyType)3)
                            selectedSoftType = (SoftBodyType)0;
                        else
                            selectedSoftType += 1;
                    }
                }
            };
            window.MouseButtonReleased += (sender, e) =>
            {
                if (e.Button == Mouse.Button.Left)
                    leftMouseHeld = false;

                if (e.Button == Mouse.Button.Right)
                    rightMouseHeld = false;
            };

            // window.MouseButtonPressed += (sender, e) =>
            // {
            //     Vector2f mousePosition = new Vector2f(e.X, e.Y);
            //     if (e.Button == Mouse.Button.Left)
            //     {
            //         CreateSoftBodyEntity(scene, mousePosition, 100f, SoftBodyType.SQUARE);
            //     }
            //
            //     if (e.Button == Mouse.Button.Right)
            //     {
            //         CreateRigidBodyEntity(scene, mousePosition, 100f, RigidBodyType.CIRCLE);
            //     }
            // };
            var fixedDeltaTime = 1.0f / 60.0f;
            float accumulatedTime = 0.0f;
            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(GameState.windowColor);

                float deltaTime = clock.Restart().AsSeconds();
                timeSinceLastCreation += deltaTime;

                accumulatedTime += deltaTime;
                float fps = 1.0f / deltaTime;


                Vector2i mousePos = Mouse.GetPosition(window);
                Vector2f mousePosition = new Vector2f(mousePos.X, mousePos.Y);
                if (leftMouseHeld && timeSinceLastCreation >= creationCooldown)
                {
                    // CreateSoftBodyEntity(scene, mousePosition, 100f, SoftBodyType.SQUARE);
                    PhysicsSystem t = scene.GetPhysicsSystem<PhysicsSystem>();

                    t.ApplyExplosionForce(mousePosition, 150f, 100f);
                }

                if (rightMouseHeld && timeSinceLastCreation >= creationCooldown)
                {
                    if (rigidbodyMode)
                        CreateRigidBodyEntity(scene, mousePosition, 100f, selectedRigidType);
                    else
                        CreateSoftBodyEntity(scene, mousePosition, 100, selectedSoftType);
                    timeSinceLastCreation = 0f;
                }


                sceneManager.Update(deltaTime);
                while (accumulatedTime >= fixedDeltaTime)
                {
                    sceneManager.FixedUpdate(fixedDeltaTime);
                    accumulatedTime -= fixedDeltaTime;
                }

                string selmode = rigidbodyMode ? "rigidbody" : "softbody";
                string selsubtype = rigidbodyMode ? selectedRigidType.ToString() : selectedSoftType.ToString();
                fpsText.DisplayedString =
                    $"FPS: {fps:F1}   \n    Entities: {scene.GetEntitiesAmount()}  \n    Mousepos: {mousePos.X} , {mousePos.Y} \n    Mode: {selmode}  \n    Type: {selsubtype}";
                window.Draw(fpsText);

                window.Display();
            }

            window.SetActive(false);
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
            entity.AddComponent(new SoftRendererComponent());
        }

        private static void CreateRigidBodyEntity(Scene scene, Vector2f position, float scale,
            RigidBodyType rigidBodyType)
        {
            Entity entity = scene.Instantiate(new Entity($"Entity_{Guid.NewGuid()}"));
            entity.AddComponent(new Transform(position, scale));
            entity.AddComponent(new RigidBodyComponent(rigidBodyType));
            entity.AddComponent(new RendererComponent(entity.GetComponent<RigidBodyComponent>().Shape));
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