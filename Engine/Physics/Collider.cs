using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Engine.Physics;

/// <summary>
/// Uses a voxel system to detect collisions.
/// </summary>
public abstract class CollisionShape
{
    public bool[,,] CollisonVoxels = new bool[0, 0, 0];

    public abstract Vector3 Bounds { get; }

    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale = Vector3.One;

    public abstract void CalculateCollisionVoxels();

    public delegate void ForVoxel(Vector3Int pos, bool voxel);

    public abstract void ForEachVoxel(ForVoxel func);

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

            Vector3 rotationDiff = shape0.Rotation - shape1.Rotation,
            diffUnit = rotationDiff.Unit == Vector3.Zero ? Vector3.Zero : rotationDiff.Unit;

            float angle = float.DegreesToRadians(rotationDiff.Magnitude);

            Vector3 pos0 = Vector3.Rotate(shape0.Position - shape1.Position, diffUnit, angle) + shape1.Position,
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

    public static bool IsColliding(CollisionShape first, CollisionShape second)
    {
        bool inBounds = IsCollidingInBounds(first, second);

        if (!inBounds)
        {
            return false;
        }
        Vector3 rotationDiff = second.Rotation - first.Rotation,
        diffUnit = rotationDiff.Unit == Vector3.Zero ? Vector3.Zero : rotationDiff.Unit;

        float angle = float.DegreesToRadians(rotationDiff.Magnitude);

        Vector3Int pos = (Vector3Int)(Vector3.Rotate(first.Position - second.Position, diffUnit, angle) + second.Position);

        bool isColliding = false;

        first.ForEachVoxel((pos0, voxel0) =>
        {
            if (!voxel0)
            {
                return;
            }

            Vector3Int localPos = pos0;

            bool inX = 0 <= localPos.X && localPos.X < second.CollisonVoxels.GetLength(0),
            inY = 0 <= localPos.Y && localPos.Y < second.CollisonVoxels.GetLength(1),
            inZ = 0 <= localPos.Z && localPos.Z < second.CollisonVoxels.GetLength(2);

            if (!inX || !inY || !inZ)
            {
                return;
            }

            bool other = second.CollisonVoxels[localPos.X, localPos.Y, localPos.Z];

            isColliding = other;
        });

        return isColliding;
    }

    public bool IsPointColliding(Vector3 point)
    {
        Vector3 diffUnit = Rotation.Unit == Vector3.Zero ? Vector3.Zero : Rotation.Unit;

        float angle = float.DegreesToRadians(Rotation.Magnitude);

        Vector3 pos = Rotation == Vector3.Zero ? Position : Vector3.Rotate(point - Position, diffUnit, -angle),
            bounds = (Scale * Bounds) + point;

        bool inX = pos.X <= point.X && point.X <= bounds.X,
            inY = pos.Y <= point.Y && point.Y <= bounds.Y,
            inZ = pos.Z <= point.Z && point.Z <= bounds.Z;

        return inX && inY && inZ;
    }
}

[SaveNode("engine.collider")]
public sealed class Collider : Node3D
{
    static public readonly List<Collider> Colliders = [];

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

        CollisionShape = shape;
        shape.CalculateCollisionVoxels();
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

    protected override void OnNonPositionUpdate()
    {
        base.OnNonPositionUpdate();

        CollisionShape?.CalculateCollisionVoxels();
    }

    public override void Awake()
    {
        base.Awake();

        CollisionShape?.CalculateCollisionVoxels();

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

            bool inBounds = CollisionShape.IsColliding(CollisionShape, other.CollisionShape);

            if (inBounds)
            {
                OnCollision?.Invoke(this, other);
                Console.WriteLine("Colliding!");
            }
        }
    }
}