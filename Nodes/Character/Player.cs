using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ZombieSurvival.Engine;

namespace ZombieSurvival.Nodes.Character;

public class Player : Character
{
    [AllowNull]
    public Camera Camera;

    public float CameraSpeed = 1.5f;
    public float Sensitivity = 0.005f;

    private bool FirstMove = true;
    private Vector2 LastPos;

    public override void Update(double delta)
    {
        base.Update(delta);

        // TODO - Simplify input using axis
        var input = Input.KeyboardState;
        float fDelta = (float)delta,
        time = fDelta * 4;

        if (input.IsKeyDown(Keys.W))
        {
            Camera.Position += Camera.Front * CameraSpeed * time; // Forward
        }
        if (input.IsKeyDown(Keys.S))
        {
            Camera.Position -= Camera.Front * CameraSpeed * time; // Backwards
        }

        if (input.IsKeyDown(Keys.A))
        {
            Camera.Position -= Camera.Right * CameraSpeed * time; // Left
        }
        if (input.IsKeyDown(Keys.D))
        {
            Camera.Position += Camera.Right * CameraSpeed * time; // Right
        }

        if (input.IsKeyDown(Keys.Space))
        {
            Camera.Position += Camera.Up * CameraSpeed * time; // Up
        }
        if (input.IsKeyDown(Keys.LeftShift))
        {
            Camera.Position -= Camera.Up * CameraSpeed * time; // Down
        }

        // Get the mouse state
        var mouse = Input.MouseState;

        if (FirstMove) // This bool variable is initially set to true.
        {
            LastPos = new Vector2(mouse.X, mouse.Y);
            FirstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            Console.WriteLine(Camera.Yaw);
            Console.WriteLine(Camera.Pitch);

            var deltaX = mouse.X - LastPos.X;
            var deltaY = mouse.Y - LastPos.Y;
            LastPos = new Vector2(mouse.X, mouse.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            Camera.Yaw += deltaX * Sensitivity;
            Camera.Pitch -= deltaY * Sensitivity; // Reversed since y-coordinates range from bottom to top

            Console.WriteLine(Camera.Yaw);
            Console.WriteLine(Camera.Pitch);
        }
    }

    public override void Awake()
    {
        base.Awake();

        Camera = New<Camera>(null, "PlayerCamera");
        Camera.IsCurrentCamera = true;
    }
}