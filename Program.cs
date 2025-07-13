using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ZombieSurvival.Engine;
using ZombieSurvival.Engine.Graphics;
using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Engine.NodeSystem.Scene;
using ZombieSurvival.Nodes;
using ZombieSurvival.Nodes.Character;

namespace ZombieSurvival;

// TODO - Write Tests
// TODO - Convert Mesh file to mesh obj
// TODO - Save mesh as a file as well as an object if avaibable

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

	public static void CreateTestScene()
	{
		_ = Node.New<Player>(null);

		MeshContainer container1 = Node.New<MeshContainer>(null);
		container1.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Triangle);
		container1.Position = EVector3.Right * 5.0f;
		container1.Rotation = EVector3.Up * 5;
		container1.Scale = EVector3.One * 2.0f;
		container1.Texture0 = "container.png";
		container1.Texture1 = "awesome.png";
		container1.Name = "Object1";

		MeshContainer container2 = Node.New<MeshContainer>(container1);
		container2.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
		container2.Texture0 = "awesome.png";
		container2.Position = EVector3.Up * 10.0f;
		container2.Name = "Object2";

		MeshContainer container0 = Node.New<MeshContainer>(null);
		container0.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Quad);
		container0.Rotation = EVector3.Right * 135;
		container0.Texture0 = "uhidsiuosaduiohdsao"; // Expected - Error Texture
		container0.Name = "Object0";

		SceneHandler.SaveScene(Tree.GetTree(), "resources/scenes/test.scene");
	}

	public static int Main(string[] args)
	{
		Tree.InitaliseTree();

		// CreateTestScene();
		SceneHandler.LoadScene(Tree.GetTree(), "resources/scenes/test.scene");

		RunWindow();

		return 0;
	}
}