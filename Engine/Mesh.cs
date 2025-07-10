using OpenTK.Graphics.OpenGL4;

namespace ZombieSurvival.Engine;

public class Mesh
{
    public enum MeshPrimitive
    {
        Triangle,
        Quad,
        Cube
    }

    public required Vector3[] Vertices;
    public required int[] Indices;
    public required Vector2[] UVs;
    public required PrimitiveType PrimitiveType;

    private static readonly Mesh TriangleMesh = new()
    {
        Vertices = [
            Vector3.Zero,
            Vector3.Right,
            Vector3.Up
        ],
        Indices = [0, 1, 2],
        UVs = [
            Vector2.Zero,
            Vector2.Right,
            Vector2.Up
        ],
        PrimitiveType = PrimitiveType.Triangles
    };

    private static readonly Mesh QuadMesh = new()
    {
        Vertices = [
            new(1, 1),
            Vector3.Right,
            Vector3.Zero,
            Vector3.Up
        ],
        Indices = [0, 1, 3, 1, 2, 3],
        UVs = [
            Vector2.One,
            Vector2.Right,
            Vector2.Zero,
            Vector2.Up
        ],
        PrimitiveType = PrimitiveType.Triangles
    };

    private static readonly Mesh CubeMesh = new()
    {
        Vertices = [
            new(0, 0, 1), //0
            new(1, 0, 1), //1
            new(0, 1, 1), //2
            new(1, 1, 1), //3
            new(0, 0, 0), //4
            new(1, 0, 0), //5
            new(0, 1, 0), //6
            new(1, 1, 0)  //7
        ],
        Indices = [
            2, 6, 7,
            2, 3, 7,

            0, 4, 5,
            0, 1, 5,

            0, 2, 6,
            0, 4, 6,

            1, 3, 7,
            1, 5, 7,

            0, 2, 3,
            0, 1, 3,

            4, 6, 7,
            4, 5, 7
        ],
        PrimitiveType = PrimitiveType.Triangles,
        UVs = []
    };

    public static Mesh GetMeshPrimitive(MeshPrimitive primative)
    {
        switch (primative)
        {
            case MeshPrimitive.Triangle:
                return TriangleMesh;
            case MeshPrimitive.Quad:
                return QuadMesh;
            case MeshPrimitive.Cube:
                return CubeMesh;
            default:
                throw new NotImplementedException($"Mesh Primative {primative} not implemented");
        }
    }

    public float[] IntoFeed()
    {
        return IntoFeed(Vector3.Zero, Vector3.One);
    }

    public float[] IntoFeed(Vector3 offset, Vector3 scale)
    {
        int feedLength = Vertices.Length * 5;
        float[] feed = new float[feedLength];
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vector3 vert = Vertices[i];
            feed[i * 5] = offset.X + vert.X * scale.X;
            feed[i * 5 + 1] = offset.Y + vert.Y * scale.Y;
            feed[i * 5 + 2] = offset.Z + vert.Z * scale.Z;

            Vector2 uv = i < UVs.Length ? UVs[i] : Vector2.Zero;
            feed[i * 5 + 3] = uv.X;
            feed[i * 5 + 4] = uv.Y;
        }
        return feed;
    }

    public Mesh() { }
}