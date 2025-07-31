using System.Diagnostics.CodeAnalysis;
using ZombieSurvival.Engine.NodeSystem;

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

    public Collider? Collider;

    private Vector3 DirectionToGetOut(out bool needToGetOut)
    {
        if (!Collider.IsColliderValid(Collider, out var _))
        {
            needToGetOut = false;
            return Vector3.Zero;
        }

        Collider[] touching = Collider.GetTouchingCollider();
        if (touching.Length == 0)
        {
            needToGetOut = false;
            return Vector3.Zero;
        }

        Vector3 total = Vector3.Zero, averageOut;

        foreach (Collider other in touching)
        {
            total += Collider.GlobalPosition - other.GlobalPosition;
        }

        averageOut = total / touching.Length;

        needToGetOut = true;
        return averageOut;
    }

    public override void UpdateFixed()
    {
        base.UpdateFixed();

        Vector3 fall = -Vector3.Up * (Mass * Gravity * AirResistance),
        final = fall * (float)Tree.FixedUpdateSeconds;

        Acceleration = final;

        Vector3 directionOut = DirectionToGetOut(out bool needToGetOut);
        if (needToGetOut)
        {
            Console.WriteLine("out");
            GlobalPosition += directionOut; //  + (-Acceleration.Unit * Scale / 2)
            return;
        }
        Console.WriteLine("in");

        GlobalPosition += Acceleration;
    }
}