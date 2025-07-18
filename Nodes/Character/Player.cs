using System.Diagnostics.CodeAnalysis;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ZombieSurvival.Engine;
using ZombieSurvival.Engine.NodeSystem.Scene;

namespace ZombieSurvival.Nodes.Character;

[SaveNode("zombie-survial.player")]
public class Player : Character
{
    [AllowNull]
    public Camera Camera;

    [Export]
    public float CameraSpeed { get; set; } = 1.5f;
    [Export]
    public float Sensitivity { get; set; } = 0.005f;

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

            Camera.Rotation += new Vector3(deltaX * Sensitivity, deltaY * Sensitivity, 0);
        }
    }

    public override void Awake()
    {
        base.Awake();

        Camera = New<Camera>(null, "PlayerCamera");
        Camera.IsCurrentCamera = true;
        Camera.Archive = false;
    }
}