using System.Text.Json.Serialization;

namespace ZombieSurvival.Engine.Physics;

public class CubeCollision : CollisionShape
{
    private Vector3 _Dimensions = Vector3.One;
    public Vector3 Dimensions
    {
        get
        {
            return _Dimensions;
        }
        set
        {
            _Dimensions = value;
            CalculateCollisionVoxels();
        }
    }

    private Vector3Int GetVoxelsPerDimension()
    {
        return (Vector3Int)(Dimensions * Scale / Physics.CollisionVoxelSize);
    }

    [JsonIgnore]
    public override Vector3 Bounds => Dimensions;

    public override void ForEachVoxel(ForVoxel func)
    {
        Vector3Int voxelSize = GetVoxelsPerDimension();
        for (int x = 0; x < voxelSize.X; x++)
        {
            for (int y = 0; y < voxelSize.Y; y++)
            {
                for (int z = 0; z < voxelSize.Z; z++)
                {
                    Vector3Int pos = new(x, y, z);

                    func(pos, CollisonVoxels[x, y, z]);
                }
            }
        }
    }

    public override void CalculateCollisionVoxels()
    {
        Vector3Int voxelsSize = GetVoxelsPerDimension();

        bool[,,] voxels = new bool[voxelsSize.Z, voxelsSize.Y, voxelsSize.Z];

        for (int x = 0; x < voxelsSize.X; x++)
        {
            for (int y = 0; y < voxelsSize.Y; y++)
            {
                for (int z = 0; z < voxelsSize.Z; z++)
                {
                    voxels[x, y, z] = true;
                }
            }
        }

        CollisonVoxels = voxels;
    }
}