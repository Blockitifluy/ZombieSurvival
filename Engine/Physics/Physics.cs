namespace ZombieSurvival.Engine.Physics;

/// <summary>
/// Contains miscellaneous methods about physics and collision.
/// </summary>
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

}