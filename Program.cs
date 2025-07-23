using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ZombieSurvival.Engine;
using ZombieSurvival.Engine.Graphics;
using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Engine.Physics;
using ZombieSurvival.Nodes;
using ZombieSurvival.Nodes.Character;

namespace ZombieSurvival;

// TODO - Write Tests
// TODO - Convert Mesh file to mesh obj

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

		Mover mover = Node.New<Mover>(null, "awesome-mover");
		mover.To = EVector3.Forward * 10;
		mover.Speed = 2f;

		MeshContainer awesomeCube = Node.New<MeshContainer>(mover, "awesome-cube");
		awesomeCube.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
		awesomeCube.Mesh.SaveResource("resources/meshs/cube.mesh");
		awesomeCube.Texture0 = "awesome.png";

		Collider collision0 = Node.New<Collider>(mover, "awe-collision");
		collision0.ApplyCollisionShape(new CubeCollision());

		MeshContainer crateCube = Node.New<MeshContainer>(null, "crate-cube");
		crateCube.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
		crateCube.Position = EVector3.Forward * 5f;
		crateCube.Texture0 = "container.png";

		Collider collision1 = Node.New<Collider>(crateCube, "crate-collision");
		collision1.ApplyCollisionShape(new CubeCollision());

		SceneHandler.SaveScene(Tree.GetTree(), "resources/scenes/demo.scene");
	}

	public const string ProgramHelp = """
	demo - Loads and save the demo level
	load [path to scene] - Loads the scene from the file
	""";

	public static int Main(string[] args)
	{
		using Tree tree = Tree.InitaliseTree();

		if (args.Length == 0)
		{
			Console.WriteLine(ProgramHelp);

			return 0;
		}

		string cmd = args[0];

		if (cmd == "demo")
		{
			CreateTestScene();
		}
		else if (cmd == "load")
		{
			string path = args[1];
			SceneHandler.LoadScene(tree, path);
		}

		RunWindow();

		return 0;
	}
}