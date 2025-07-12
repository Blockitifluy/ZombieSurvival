using ZombieSurvival.Engine;
using ZombieSurvival.Engine.Graphics;
using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Nodes;

/// <summary>
/// Uses a mesh and textures to be rendered.
/// </summary>
public class MeshContainer : Node3D
{
    public Mesh? Mesh;

    private string _Texture0 = "";
    public string Texture0
    {
        get => _Texture0;
        set
        {
            _Texture0 = value;
            _Textures[0] = value;
        }
    }

    private string _Texture1 = "";
    public string Texture1
    {
        get => _Texture1;
        set
        {
            _Texture1 = value;
            _Textures[1] = value;
        }
    }

    private readonly string[] _Textures = ["", ""];
    public string[] Textures => _Textures;
}

