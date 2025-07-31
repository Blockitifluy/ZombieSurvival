using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Engine.Physics;

/// <summary>
/// Uses a voxel system to detect collisions.
/// </summary>
public abstract class CollisionShape
{
    /// <summary>
    /// All of the voxels of the Collision Shape
    /// </summary>
    public bool[,,] CollisonVoxels = new bool[0, 0, 0];

    [JsonIgnore]
    public abstract Vector3 Bounds { get; }

    public Vector3 Position;
    public Vector3 Rotation;
    public Vector3 Scale = Vector3.One;

    public abstract void CalculateCollisionVoxels();

    public delegate bool ForVoxel(Vector3Int pos, bool voxel);

    /// <summary>
    /// Loops over every voxel, firing <paramref name="func"/>.
    /// </summary>
    /// <param name="func">Delegate to be fired every voxel.</param>
    public void ForEachVoxel(ForVoxel func)
    {
        Vector3Int voxelSize = GetVoxelsPerDimension();
        bool stop = false;

        for (int x = 0; x < voxelSize.X; x++)
        {
            for (int y = 0; y < voxelSize.Y; y++)
            {
                for (int z = 0; z < voxelSize.Z; z++)
                {
                    Vector3Int pos = new(x, y, z);

                    stop = func(pos, CollisonVoxels[x, y, z]);
                }

                if (stop)
                {
                    break;
                }
            }

            if (stop)
            {
                break;
            }
        }
    }

    public Vector3Int GetVoxelsPerDimension()
    {
        return (Vector3Int)(Bounds * Scale / Physics.CollisionVoxelSize);
    }

    /// <summary>
    /// Checks if two Collision Shapes' bounds are intersecting. 
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

            Vector3 rotDiff = shape0.Rotation - shape1.Rotation;

            Vector3 pos0 = Vector3.RotateEuler(shape0.Position - shape1.Position, rotDiff) + shape1.Position,
            pos1 = shape1.Position,
            bounds0 = (shape0.Scale * shape0.Bounds) + pos0,
            bounds1 = (shape1.Scale * shape1.Bounds) + pos1;

            bool inside = Physics.InPointInside(pos0, pos1, bounds1) || Physics.InPointInside(bounds0, pos1, bounds1);

            if (inside)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if two Collision Shapes are inside or colliding.
    /// </summary>
    /// <remarks>
    /// More presice than <see cref="IsCollidingInBounds"/>.
    /// </remarks>
    /// <param name="first">The first collision shape.</param>
    /// <param name="second">The second collision shape.</param>
    /// <returns>True, if two collision shapes are intersection.</returns>
    public static bool IsColliding(CollisionShape first, CollisionShape second)
    {
        bool inBounds = IsCollidingInBounds(first, second);

        if (!inBounds)
        {
            return false;
        }

        Vector3 rotDiff = second.Rotation - first.Rotation;

        Vector3Int pos = (Vector3Int)((Vector3.RotateEuler(second.Position - first.Position, rotDiff) + first.Position) / Physics.CollisionVoxelSize);

        bool isColliding = false;
        first.ForEachVoxel((pos0, voxel0) =>
        {
            if (!voxel0)
            {
                return false;
            }

            Vector3Int localPos = pos0;

            bool inside = Physics.InPointInside(localPos, second.GetVoxelsPerDimension());
            if (!inside)
            {
                return false;
            }

            bool other = second.CollisonVoxels[localPos.X, localPos.Y, localPos.Z];

            if (other)
            {
                isColliding = true;
                return true;
            }

            return false;
        });

        return isColliding;
    }

    /// <summary>
    /// Checks if a point is intersecting with the collider.
    /// </summary>
    /// <param name="point">A point</param>
    /// <returns>True, if a point is intersecting.</returns>
    public bool IsPointCollidingInBounds(Vector3 point)
    {
        Vector3 bounds = Scale * Bounds,
        pos = point - Position;

        return Physics.InPointInside(pos, bounds);
    }

    /// <summary>
    /// Checks if a point is inside or colliding with Collision Shapes.
    /// </summary>
    /// <param name="point">A point</param>
    /// <returns>True, if a point is inside or colliding.</returns>
    public bool IsPointColliding(Vector3 point)
    {
        if (!IsPointCollidingInBounds(point))
        {
            return false;
        }

        Vector3 posRot = point - Position;

        Vector3Int intStep = (Vector3Int)(posRot * Physics.CollisionVoxelSize);
        bool colliding = CollisonVoxels[intStep.X, intStep.Y, intStep.Z];

        return colliding;
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

    public static bool IsColliderValid([NotNullWhen(true)] Collider? collider, [NotNullWhen(true)] out CollisionShape? shape)
    {
        if (collider is null)
        {
            shape = null;
            return false;
        }

        if (collider.CollisionShape is null)
        {
            shape = null;
            return false;
        }

        shape = collider.CollisionShape;
        return true;
    }

    public override void Awake()
    {
        base.Awake();

        CollisionShape?.CalculateCollisionVoxels();

        Colliders.Add(this);
    }

    public Collider[] GetTouchingCollider() // TODO - Add filter
    {
        List<Collider> colliders = [];

        if (!IsColliderValid(this, out var shape))
        {
            return [];
        }

        foreach (Collider other in Colliders)
        {
            if (other == this)
            {
                continue;
            }

            if (!IsColliderValid(other, out var otherShape))
            {
                continue;
            }

            bool inBounds = CollisionShape.IsColliding(shape, otherShape);

            if (inBounds)
            {
                colliders.Add(other);
            }
        }

        return [.. colliders];
    }

    public override void UpdateFixed()
    {
        base.UpdateFixed();

        var touching = GetTouchingCollider();
        foreach (var collider in touching)
        {
            OnCollision?.Invoke(this, collider);
        }
    }
}