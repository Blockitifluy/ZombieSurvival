using OpenTK.Mathematics;
using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Nodes;

public class Node3D : Node
{
    private EVector3 _Rotation = EVector3.Up * 90;
    public EVector3 Position = EVector3.Zero;
    public EVector3 Rotation
    {
        get
        {
            return _Rotation;
        }
        set
        {
            _Rotation = value;
            UpdateVectors();
        }
    }
    public EVector3 Scale = EVector3.One;

    private EVector3 _Front = -EVector3.Forward;
    private EVector3 _Up = EVector3.Up;
    private EVector3 _Right = -EVector3.Right;

    public EVector3 Front => _Front;
    public EVector3 Up => _Up;
    public EVector3 Right => _Right;

    public float Pitch
    {
        get => Rotation.Y;
        set
        {
            // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
            // of weird "bugs" when you are using euler angles for rotation.
            // If you want to read more about this you can try researching a topic called gimbal lock
            var angle = MathHelper.Clamp(value, -89f, 89f);
            _Rotation.Y = angle;
            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => Rotation.X;
        set
        {
            _Rotation.X = value;
            UpdateVectors();
        }
    }

    private void UpdateVectors()
    {
        // First, the front matrix is calculated using some basic trigonometry.
        _Front.X = MathF.Cos(Pitch) * MathF.Cos(Yaw);
        _Front.Y = MathF.Sin(Pitch);
        _Front.Z = MathF.Cos(Pitch) * MathF.Sin(Yaw);

        // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
        _Front = _Front.Unit;

        // Calculate both the right and the up vector using cross product.
        // Note that we are calculating the right from the global up; this behaviour might
        // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
        _Right = EVector3.Cross(_Front, EVector3.Up).Unit;
        _Up = EVector3.Cross(_Right, _Front).Unit;
    }
}