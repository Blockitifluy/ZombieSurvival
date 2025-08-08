using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Nodes;

namespace ZombieSurvival.Engine.Physics;

public enum CollisionFilter
{
    Include,
    Exclude
}

public interface ICollisionFilter
{
    /// <summary>
    /// Controls how the <see cref="FilterList"/> behaves.
    /// </summary>
    public CollisionFilter FilterType { get; set; }
    /// <summary>
    /// The nodes and it's desendants that are included/excluded depending on <see cref="FilterType"/>.
    /// </summary>
    public Node[] FilterList { get; set; }
    public string CollisionGroup { get; set; }
}

/// <summary>
/// Contains miscellaneous methods about physics and collision.
/// </summary>
[SavableSingleton("Physics")]
public static partial class Physics
{
    public static bool InPointInside(Vector3Int point, Vector3Int min, Vector3Int max)
    {
        bool inX = min.X <= point.X && point.X < max.X,
        inY = min.Y <= point.Y && point.Y < max.Y,
        inZ = min.Z <= point.Z && point.Z < max.Z;

        return inX && inY && inZ;
    }

    public static bool InPointInside(Vector3Int point, Vector3Int max)
    {
        return InPointInside(point, Vector3Int.Zero, max);
    }

    public static bool InPointInside(Vector3 point, Vector3 min, Vector3 max)
    {
        bool inX = min.X <= point.X && point.X < max.X,
        inY = min.Y <= point.Y && point.Y < max.Y,
        inZ = min.Z <= point.Z && point.Z < max.Z;

        return inX && inY && inZ;
    }

    public static bool InPointInside(Vector3 point, Vector3 max)
    {
        return InPointInside(point, Vector3.Zero, max);
    }

    /// <summary>
    /// Gets all of the corners of a collider.
    /// </summary>
    /// <param name="collider">The collider</param>
    /// <returns>All of the corners (acounting for transformations).</returns>
    public static Vector3[] GetCornersOfCollider(Collider collider)
    {
        CollisionShape? shape = collider.CollisionShape;

        ArgumentNullException.ThrowIfNull(shape, nameof(shape));

        Vector3 pos = collider.GlobalPosition,
        scale = collider.GlobalScale * shape.Bounds,
        rot = collider.GlobalRotation;

        Vector3[] local = [
            Vector3.Zero, // Bottom Left Back
            Vector3.Up * scale, // Top Left Back

            Vector3.Right * scale, // Bottom Right Back
            new Vector3(scale.X, scale.Y, 0), // Top Right Back

            Vector3.Forward * scale, // Bottom Left Front
            new Vector3(0, scale.Y, scale.Z), // Top Left Front

            new Vector3(scale.X, 0, scale.Z), // Bottom Right Front
            Vector3.One * scale // Top Right Front
        ];

        Vector3[] corners = new Vector3[local.Length];

        int i = 0;
        foreach (Vector3 crn in local)
        {
            corners[i] = pos + Vector3.RotateEuler(crn, rot);
            i++;
        }

        return corners;
    }

    public static bool IncludedInFilter(Collider collider, ICollisionFilter filter)
    {
        if (!CanCollideWith(collider.CollisionGroup, filter.CollisionGroup))
        {
            return false;
        }

        foreach (Node node in filter.FilterList)
        {
            bool isDescendant = collider.IsDescendant(node) || node == collider;
            if (isDescendant)
            {
                return filter.FilterType == CollisionFilter.Include;
            }
        }

        return filter.FilterType == CollisionFilter.Exclude;
    }
}