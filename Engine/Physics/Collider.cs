using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Engine.NodeSystem.Scene;

namespace ZombieSurvival.Engine.Physics;

/// <summary>
/// Uses a voxel system to detect collisions.
/// </summary>
public abstract class CollisionShape
{
    public const float CollisionVoxelSize = 0.1f;

    public bool[] CollisonVoxels = [];

    public abstract Vector3 Bounds { get; }

    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale = Vector3.One;

    /// <summary>
    /// Checks if two Collider Shapes' bounds are intersecting. 
    /// </summary>
    /// <param name="first">The first collision shape.</param>
    /// <param name="second">The second collision shape.</param>
    /// <returns>True, if two collision shapes are intersecting.</returns>
    public static bool IsCollidingInBounds(CollisionShape first, CollisionShape second)
    {
        for (int cube = 0; cube < 2; cube++)
        {
            CollisionShape shape0 = cube == 0 ? first : second,
            shape1 = cube == 0 ? second : first;

            Vector3 pos0 = shape0.Position,
            pos1 = shape1.Position,
            bounds0 = (shape0.Scale * shape0.Bounds) + pos0,
            bounds1 = (shape1.Scale * shape1.Bounds) + pos1;

            bool inX = (pos1.X <= pos0.X && pos0.X <= bounds1.X) || (pos1.X <= bounds0.X && bounds0.X <= bounds1.X),
            inY = (pos1.Y <= pos0.Y && pos0.Y <= bounds1.Y) || (pos1.Y <= bounds0.Y && bounds0.Y <= bounds1.Y),
            inZ = (pos1.Z <= pos0.Z && pos0.Z <= bounds1.Z) || (pos1.Z <= bounds0.Z && bounds0.Z <= bounds1.Z);
            if (inX && inY && inZ)
            {
                return true;
            }
        }

        return false;
    }

    // TODO
    public static bool IsColliding(CollisionShape first, CollisionShape second)
    {
        throw new NotImplementedException();
    }
}

[SaveNode("engine.collider")]
public sealed class Collider : Node3D
{
    static private readonly List<Collider> Colliders = [];

    private CollisionShape? _CollisionShape;
    [Export]
    public CollisionShape? CollisionShape
    {
        get => _CollisionShape;
        set { _CollisionShape = value; }
    }
    [Export]
    public bool ChangeShapeTransform { get; set; } = true;

    public event EventHandler<Collider>? OnCollision;

    public void ApplyCollisionShape(CollisionShape shape)
    {
        shape.Rotation = GlobalRotation;
        shape.Scale = GlobalScale;
        shape.Position = GlobalPosition;

        _CollisionShape = shape;
    }

    protected override void UpdateTransformations()
    {
        base.UpdateTransformations();

        if (ChangeShapeTransform && CollisionShape != null)
        {
            CollisionShape.Rotation = GlobalRotation;
            CollisionShape.Scale = GlobalScale;
            CollisionShape.Position = GlobalPosition;
        }
    }

    public override void Awake()
    {
        base.Awake();

        Colliders.Add(this);
    }

    public override void UpdateFixed()
    {
        base.UpdateFixed();

        foreach (Collider other in Colliders)
        {
            if (other == this)
            {
                continue;
            }

            if (CollisionShape is null || other.CollisionShape is null)
            {
                continue;
            }

            bool inBounds = CollisionShape.IsCollidingInBounds(CollisionShape, other.CollisionShape);

            if (inBounds)
            {
                OnCollision?.Invoke(this, other);
                Console.WriteLine("Colliding!");
            }
        }
    }
}