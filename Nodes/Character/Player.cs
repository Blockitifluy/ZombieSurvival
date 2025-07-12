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

        float fDelta = (float)delta,
        time = fDelta * 4;

        Vector3 xMovement = Camera.Right * Input.InputAxis(Keys.A, Keys.D)
        * CameraSpeed * time;
        Vector3 yMovement = Camera.Up * Input.InputAxis(Keys.LeftShift, Keys.Space)
        * CameraSpeed * time;
        Vector3 zMovement = Camera.Front * Input.InputAxis(Keys.S, Keys.W)
        * CameraSpeed * time;

        Camera.Position += xMovement + yMovement + zMovement;

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
            var deltaX = mouse.X - LastPos.X;
            var deltaY = mouse.Y - LastPos.Y;
            LastPos = new Vector2(mouse.X, mouse.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            Camera.Yaw += deltaX * Sensitivity;
            Camera.Pitch -= deltaY * Sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }

    public override void Awake()
    {
        base.Awake();

        Camera = New<Camera>(null, "PlayerCamera");
        Camera.IsCurrentCamera = true;
    }
}