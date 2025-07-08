using OpenTK.Graphics.OpenGL4;

namespace ZombieSurvival.Engine;

public class Mesh
{
    public enum MeshPrimative
    {
        Triangle,
        Quad
    }

    public required Vector3[] Vertices;
    public required int[] Indicies;
    public required Vector2[] UVs;
    public required PrimitiveType PrimitiveType;

    private static readonly Mesh TriangleMesh = new()
    {
        Vertices = [
            Vector3.Zero,
            Vector3.Right,
            Vector3.Up
        ],
        Indicies = [0, 1, 2],
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
            Vector3.One,
            Vector3.Right,
            Vector3.Zero,
            Vector3.Up
        ],
        Indicies = [0, 1, 3, 1, 2, 3],
        UVs = [
            Vector2.One,
            Vector2.Right,
            Vector2.Zero,
            Vector2.Up
        ],
        PrimitiveType = PrimitiveType.Triangles
    };

    public static Mesh GetMeshPrimative(MeshPrimative primative)
    {
        switch (primative)
        {
            case MeshPrimative.Triangle:
                return TriangleMesh;
            case MeshPrimative.Quad:
                return QuadMesh;
            default:
                throw new NotImplementedException($"Mesh Primative {primative} not implemented");
        }
    }

    public float[] IntoFeed()
    {
        int feedLength = Vertices.Length * 5;
        float[] feed = new float[feedLength];
        for (int i = 0; i < Vertices.Length; i++)
        {
            Vector3 vert = Vertices[i];
            feed[i * 5] = vert.X;
            feed[i * 5 + 1] = vert.Y;
            feed[i * 5 + 2] = vert.Z;

            Vector2 uv = i < UVs.Length ? UVs[i] : Vector2.Zero;
            feed[i * 5 + 3] = uv.X;
            feed[i * 5 + 4] = uv.Y;
        }
        return feed;
    }

    public Mesh() { }
}