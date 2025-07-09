using OpenTK.Mathematics;
using ZombieSurvival.Engine;

namespace ZombieSurvival.Nodes;

public sealed class Camera : Node3D
{
    private float _Fov = 90;
    public float AspectRatio;

    public static Camera? CurrentCamera { get; set; } = null;

    public bool IsCurrentCamera
    {
        get
        {
            return this == CurrentCamera;
        }
        set
        {
            if (IsCurrentCamera && !value)
            {
                CurrentCamera = null;
            }
            else if (!IsCurrentCamera && value)
            {
                CurrentCamera = this;
            }
        }
    }

    public float Fov
    {
        get => _Fov;
        set
        {
            var angle = MathHelper.Clamp(value, 1f, 90f);
            _Fov = angle;
        }
    }

    public Matrix4 GetViewMatrix()
    {
        return Matrix4.LookAt((GLVector3)Position, (GLVector3)(Position + Front), (GLVector3)Up);
    }

    public Matrix4 GetProjectionMatrix()
    {
        var rFov = MathHelper.DegreesToRadians(_Fov);
        return Matrix4.CreatePerspectiveFieldOfView(rFov, AspectRatio, 0.01f, 100f);
    }
}

