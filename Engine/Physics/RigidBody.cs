using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Engine.NodeSystem.Scene;

namespace ZombieSurvival.Engine.Physics;

[SaveNode("engine.rigid-body")]
public sealed class RigidBody : Node3D
{
    [Export]
    public float Mass { get; set; } = 1.0f;
    [Export]
    public float Gravity { get; set; } = 9.8f;

    [Export]
    public float AirResistance { get; set; } = 0.95f;

    public Vector3 Acceleration;

    public override void Update(double delta)
    {
        base.Update(delta);

        Vector3 fall = -Vector3.Up * Mass * Gravity;

        Vector3 final = fall * AirResistance;

        Acceleration += final;
        Position += Acceleration;
    }
}