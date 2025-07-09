using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ZombieSurvival.Engine;
using ZombieSurvival.Engine.Graphics;
using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Nodes;
using ZombieSurvival.Nodes.Character;

namespace ZombieSurvival;

public static class Program
{
	public static Window? GameWindow { get; set; }

	public static void RunWindow()
	{
		var nativeWindowSettings = new NativeWindowSettings()
		{
			ClientSize = new Vector2i(1024, 1024),
			Title = "Zombie Survival",
			// This is needed to run on macos
			Flags = ContextFlags.ForwardCompatible,
		};

		Console.WriteLine("Started Window");

		using (GameWindow = new(GameWindowSettings.Default, nativeWindowSettings))
		{
			GameWindow.Run();
		}
	}

	public static int Main(string[] args)
	{
		Tree.InitaliseTree();

		// TODO - Load From File

		_ = Node.New<Player>(null);

		MeshContainer container0 = Node.New<MeshContainer>(null);
		container0.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Quad);
		container0.Rotation = EVector3.Right * 135;

		MeshContainer container1 = Node.New<MeshContainer>(null);
		container1.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Triangle);
		container1.Position = EVector3.Right * 5.0f;
		container0.Rotation = EVector3.Up * 5;
		container1.Scale = EVector3.One * 5.0f;

		// MeshContainer container2 = Node.New<MeshContainer>(null);
		// container2.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
		// container2.Position = EVector3.Right * 10.0f;


		RunWindow();

		return 0;
	}
}