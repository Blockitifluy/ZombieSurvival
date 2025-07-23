using ZombieSurvival.Engine.NodeSystem;

namespace ZombieSurvival.Engine;

[SaveNode("engine.mesh-container")]
/// <summary>
/// Uses a mesh and textures to be rendered.
/// </summary>
public sealed class MeshContainer : Node3D
{
    [Export]
    public Mesh? Mesh { get; set; }

    private string _Texture0 = "";
    [Export]
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
    [Export]
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

