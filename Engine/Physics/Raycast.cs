using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Engine.Physics;

/// <summary>
/// The result of a raycast being fired.
/// </summary>
public struct RaycastResult
{
    /// <summary>
    /// The collider hit.
    /// </summary>
    public required Collider Target;
    /// <summary>
    /// On where the ray hit on the collider.
    /// </summary>
    public required Vector3 Hit;

    public override readonly string ToString()
    {
        return $"Raycast Result {Target} {Hit}";
    }
}

/// <summary>
/// Contains information about a ray.
/// </summary>
/// <param name="origin"><inheritdoc cref="Origin" path="/summary"/></param>
/// <param name="dir"><inheritdoc cref="Direction" path="/summary"/></param>
public struct Ray(Vector3 origin, Vector3 dir)
{
    public enum RaycastFilter
    {
        Include,
        Exclude
    }

    /// <summary>
    /// The starting point of the ray.
    /// </summary>
    public Vector3 Origin = origin;
    /// <summary name="dir">
    /// The direction of the ray (Magnitude means range of ray).
    /// </summary>
    public Vector3 Direction = dir;

    /// <summary>
    /// Controls how the <see cref="FilterList"/> behaves.
    /// </summary>
    public RaycastFilter FilterType = RaycastFilter.Exclude;
    /// <summary>
    /// The nodes and it's desendants that are included/excluded depending on <see cref="FilterType"/>.
    /// </summary>
    public Node[] FilterList = [];

    public override readonly string ToString()
    {
        return $"Ray origin {Origin} direction {Direction}";
    }
}


public static partial class Physics
{
    /// <summary>
    /// How big are voxels for collidision.
    /// </summary>
    public const float CollisionVoxelSize = 0.2f;

    private static bool IsIn180Sight(Vector3 origin, Vector3 direction, Collider collider)
    {
        Vector3[] corners = GetCornersOfCollider(collider);

        foreach (Vector3 corn in corners)
        {
            Vector3 cornDir = (corn - origin).Unit;

            float dot = Vector3.Dot(direction.Unit, cornDir),
            angle = float.Acos(dot);

            if (angle <= float.Pi) // angle <= 180 degress
            {
                return true;
            }
        }

        return false;
    }

    private static bool IncludedInFilter(Collider collider, Node[] filterList, Ray.RaycastFilter filter)
    {
        foreach (Node node in filterList)
        {
            bool isDescendant = node.IsDescendant(collider) || node == collider;
            if (isDescendant && filter == Ray.RaycastFilter.Include)
            {
                return true;
            }
        }

        return filter == Ray.RaycastFilter.Exclude;
    }

    /// <summary>
    /// Gets all objects that have collided with a ray.
    /// </summary>
    /// <param name="ray">The ray object</param>
    /// <returns>The array of objects hit by the ray (from closest to farthest).</returns>
    /// <exception cref="ArgumentOutOfRangeException">Direction is zero</exception>
    public static RaycastResult[] RaycastList(Ray ray)
    {
        List<Collider> colliders = Collider.Colliders;
        List<RaycastResult> hitTarget = [];

        Vector3Int intOrigin = (Vector3Int)ray.Origin;

        if (ray.Direction.X == 0 && ray.Direction.Y == 0 && ray.Direction.Z == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ray), "Raycast in zero direction");
        }

        foreach (Collider collider in colliders)
        {
            if (!IncludedInFilter(collider, ray.FilterList, ray.FilterType))
            {
                continue;
            }

            Vector3 pos = collider.GlobalPosition,
            scale = collider.GlobalScale;

            bool inSight = IsIn180Sight(ray.Origin, ray.Direction, collider);
            if (!inSight)
            {
                continue;
            }

            CollisionShape? shape = collider.CollisionShape;
            if (shape == null)
            {
                continue;
            }

            Vector3 step = Vector3.Zero,
            globalStep = Vector3.Zero;

            bool enteredOnce = false;

            while (globalStep.Magnitude < ray.Direction.Magnitude)
            {
                globalStep += ray.Direction.Unit * CollisionVoxelSize;
                step += ray.Direction.Unit;
                bool isInside = shape.IsPointCollidingInBounds(globalStep + ray.Origin);
                if (!isInside)
                {
                    if (enteredOnce)
                    {
                        break;
                    }
                    continue;
                }

                enteredOnce = true;

                bool isColliding = shape.IsPointColliding(globalStep + ray.Origin);

                if (isColliding)
                {
                    hitTarget.Add(new()
                    {
                        Hit = ray.Origin + globalStep,
                        Target = collider
                    });
                    break;
                }
            }
        }

        hitTarget.Sort((x, y) => (int)(y.Hit.Magnitude * 100 - x.Hit.Magnitude * 100));

        return [.. hitTarget];
    }

    /// <summary>
    /// Gets the first object that has collided with a ray.
    /// </summary>
    /// <param name="ray">The ray object</param>
    /// <exception cref="ArgumentOutOfRangeException">Direction is zero</exception>
    /// <returns>The first object hit</returns>
    public static RaycastResult? Raycast(Ray ray)
    {
        var rays = RaycastList(ray);

        if (rays.Length == 0)
        {
            return null;
        }

        return rays[0];
    }

    /// <inheritdoc cref="RaycastList(Ray)"/>
    /// <param name="origin"><inheritdoc cref="Ray.Origin" path="/summary"/></param>
    /// <param name="direction"><inheritdoc cref="Ray.Direction" path="/summary"/></param>
    /// <param name="filterList"><inheritdoc cref="Ray.FilterList" path="/summary"/></param>
    /// <param name="filterType"><inheritdoc cref="Ray.FilterType" path="/summary"/></param>
    public static RaycastResult[] RaycastList(Vector3 origin, Vector3 direction, IEnumerable<Node> filterList, Ray.RaycastFilter filterType = Ray.RaycastFilter.Exclude)
    {
        Ray ray = new(origin, direction)
        {
            FilterType = filterType,
            FilterList = [.. filterList]
        };

        return RaycastList(ray);
    }

    /// <inheritdoc cref="Raycast(Ray)"/>
    /// <param name="origin"><inheritdoc cref="Ray.Origin" path="/summary"/></param>
    /// <param name="direction"><inheritdoc cref="Ray.Direction" path="/summary"/></param>
    /// <param name="filterList"><inheritdoc cref="Ray.FilterList" path="/summary"/></param>
    /// <param name="filterType"><inheritdoc cref="Ray.FilterType" path="/summary"/></param>
    public static RaycastResult? Raycast(Vector3 origin, Vector3 direction, IEnumerable<Node> filterList, Ray.RaycastFilter filterType = Ray.RaycastFilter.Exclude)
    {
        Ray ray = new(origin, direction)
        {
            FilterType = filterType,
            FilterList = [.. filterList]
        };

        return Raycast(ray);
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
}