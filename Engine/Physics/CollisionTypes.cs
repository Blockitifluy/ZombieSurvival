namespace ZombieSurvival.Engine.Physics;

public class CubeCollision : CollisionShape
{
    public Vector3 Dimensions = Vector3.One;
    public override Vector3 Bounds => Dimensions;
}