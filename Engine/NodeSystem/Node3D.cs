using OpenTK.Mathematics;
using ZombieSurvival.Engine.NodeSystem.Scene;

namespace ZombieSurvival.Engine.NodeSystem;

[SaveNode("engine.node3d")]
public class Node3D : Node
{
    private EVector3 _GlobalPosition;
    private EVector3 _GlobalRotation;
    private EVector3 _GlobalScale;
    private Quaternion _GlobalQuaternion;

    private Quaternion _Quaternion;
    private EVector3 _Position = EVector3.Zero;
    private EVector3 _Rotation = EVector3.Zero;
    private EVector3 _Scale = EVector3.One;

    [Export]
    public EVector3 Position
    {
        get => _Position;
        set
        {
            _Position = value;
            UpdateTransformations();
        }
    }
    [Export]
    public EVector3 Rotation
    {
        get => _Rotation;
        set
        {
            _Rotation = value;
            UpdateVectors();
            UpdateTransformations();
        }
    }
    [Export]
    public EVector3 Scale
    {
        get => _Scale;
        set
        {
            _Scale = value;
            UpdateVectors();
            UpdateTransformations();
        }
    }
    public Quaternion Quaternion => _Quaternion;

    public EVector3 GlobalPosition => _GlobalPosition;
    public EVector3 GlobalRotation => _GlobalRotation;
    public EVector3 GlobalScale => _GlobalScale;
    public Quaternion GlobalQuaternion => _GlobalQuaternion;

    private EVector3 _Front = EVector3.Forward;
    private EVector3 _Up = EVector3.Up;
    private EVector3 _Right = -EVector3.Right;

    public EVector3 Front => _Front;
    public EVector3 Up => _Up;
    public EVector3 Right => _Right;

    /// <summary>
    /// Y Rotation
    /// </summary>
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

    /// <summary>
    /// X rotation
    /// </summary>
    public float Yaw
    {
        get => Rotation.X;
        set
        {
            _Rotation.X = value;
            UpdateVectors();
        }
    }

    protected override void OnParent(Node? futureParent)
    {
        base.OnParent(futureParent);

        UpdateTransformations();
    }

    public override void Start()
    {
        base.Start();

        UpdateTransformations();
        UpdateVectors();
    }

    private void UpdateTransformations()
    {
        Vector3 gPosition = Position,
        gRotation = Rotation,
        gScale = Scale;
        Node3D current = this;

        _Quaternion = new(Rotation.X, Rotation.Y, Rotation.Z);

        while (current.Parent is not null)
        {
            if (current.Parent is not Node3D node3D)
            {
                continue;
            }

            gPosition += node3D.Position;
            gRotation += node3D.Rotation;
            gScale += node3D.Scale;
            current = node3D;
        }

        _GlobalPosition = gPosition;
        _GlobalRotation = gRotation;
        _GlobalScale = gScale;
        _GlobalQuaternion = new(
            gRotation.X,
            gRotation.Y,
            gRotation.Z
        );
    }

    /// <summary>
    /// Updates the Front, Up and Right vectors.
    /// </summary>
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