using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Engine.Physics;

public struct RaycastResult
{
    public required Collider Target;
    public required Vector3 Hit;

    public override readonly string ToString()
    {
        return $"Raycast Result {Target} {Hit}";
    }
}

public static partial class Physics
{
    public const float CollisionVoxelSize = 0.1f;

    public enum RaycastFilter
    {
        Include,
        Exclude
    }

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

    private static bool IncludedInFilter(Collider collider, List<Node> filterList, RaycastFilter filter)
    {
        foreach (Node node in filterList)
        {
            bool isDescendant = node.IsDescendant(collider) || node == collider;
            if (isDescendant && filter == RaycastFilter.Include)
            {
                return true;
            }
        }

        return filter == RaycastFilter.Exclude;
    }

    public static RaycastResult[] RaycastList(Vector3 origin, Vector3 direction, List<Node> filterList, RaycastFilter filter = RaycastFilter.Exclude)
    {
        List<Collider> colliders = Collider.Colliders;
        List<RaycastResult> hitTarget = [];

        Vector3Int intOrigin = (Vector3Int)origin;

        if (direction.X == 0 && direction.Y == 0 && direction.Z == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(direction), "Raycast in zero direction");
        }

        foreach (Collider collider in colliders)
        {
            if (!IncludedInFilter(collider, filterList, filter))
            {
                continue;
            }

            Vector3 pos = collider.GlobalPosition,
            scale = collider.GlobalScale;

            bool inSight = IsIn180Sight(origin, direction, collider);
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

            Console.WriteLine(collider);
            while (globalStep.Magnitude < direction.Magnitude)
            {
                Console.WriteLine("step");
                globalStep += direction.Unit * CollisionVoxelSize;
                bool isInside = shape.IsPointColliding(globalStep + origin);
                if (!isInside)
                {
                    if (enteredOnce)
                    {
                        break;
                    }
                    continue;
                }

                step += direction.Unit;

                enteredOnce = true;

                Vector3Int intStep = new(
                    (int)MathF.Abs(step.X),
                    (int)MathF.Abs(step.Y),
                    (int)MathF.Abs(step.Z)
                );
                Console.WriteLine(intStep);
                Console.WriteLine(globalStep);
                Console.WriteLine(step);
                Console.WriteLine("\n");

                bool colliding = shape.CollisonVoxels[intStep.X, intStep.Y, intStep.Z];
                if (colliding)
                {
                    hitTarget.Add(new()
                    {
                        Hit = collider.GlobalPosition - globalStep,
                        Target = collider
                    });
                    break;
                }
            }
        }

        hitTarget.Sort((x, y) => (int)(y.Hit.Magnitude * 100 - x.Hit.Magnitude * 100));

        return [.. hitTarget];
    }

    public static RaycastResult? Raycast(Vector3 origin, Vector3 direction, List<Node> ignoreList, RaycastFilter ignore)
    {
        var rays = RaycastList(origin, direction, ignoreList, ignore);

        if (rays.Length == 0)
        {
            return null;
        }

        return rays[0];
    }

    public static Vector3[] GetCornersOfCollider(Collider collider)
    {
        Vector3 pos = collider.GlobalPosition,
        scale = collider.GlobalScale,
        rot = collider.GlobalRotation;

        Vector3[] local = [
            pos, // Bottom Left Back
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
            float angle = float.DegreesToRadians(rot.Magnitude);

            corners[i] = pos + Vector3.Rotate(crn, rot.Unit, angle);
            i++;
        }

        return corners;
    }
}