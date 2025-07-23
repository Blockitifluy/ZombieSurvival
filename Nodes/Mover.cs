using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Engine;

namespace ZombieSurvival.Nodes;

[SaveNode("misc.mover")]
public class Mover : Node3D
{
    [Export]
    public Vector3 From { get; set; }
    [Export]
    public Vector3 To { get; set; }

    [Export]
    public float Speed { get; set; }

    public override void Start()
    {
        base.Start();

        From = Position;
    }

    public override void Update(double delta)
    {
        base.Update(delta);

        Vector3 dir = (To - From).Unit;

        float remainingDist = (To - Position).Magnitude,
        plannedDist = (float)delta * Speed;
        float distance = MathF.Min(plannedDist, remainingDist);

        Position += dir * distance;

        if (distance == remainingDist)
        {
            (From, To) = (To, From);
        }
    }
}