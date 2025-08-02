using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ZombieSurvival.Engine;
using ZombieSurvival.Engine.Graphics;
using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Engine.Physics;
using ZombieSurvival.Nodes.Character;

namespace ZombieSurvival;

// TODO - Write Tests
// TODO - Text Rendering

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

		RigidBody rigid = Node.New<RigidBody>(null, "awe-body");
		rigid.GlobalPosition = EVector3.Up * 50.0f;
		rigid.Mass = 0.5f;

		MeshContainer awesomeCube = Node.New<MeshContainer>(rigid, "awesome-cube");
		awesomeCube.Mesh = Resource.LoadResourceFromFile<Mesh>("resources/meshs/cup.mesh");

		Collider collision0 = Node.New<Collider>(rigid, "awe-collision");
		collision0.ApplyCollisionShape(new CubeCollision());
		rigid.Collider = collision0;

		MeshContainer crateCube = Node.New<MeshContainer>(null, "crate-cube");
		crateCube.Mesh = Mesh.GetMeshPrimitive(Mesh.MeshPrimitive.Cube);
		crateCube.Texture0 = "container.png";

		Collider collision1 = Node.New<Collider>(crateCube, "crate-collision");
		collision1.ApplyCollisionShape(new CubeCollision());

		SceneHandler.SaveScene(Tree.GetTree(), "resources/scenes/collider.scene");
	}

	public const string ProgramHelp = """
	demo - Loads and save the demo level
	load [path to scene] - Loads the scene from the file
	""";

	public static int Main(string[] args)
	{
		using Tree tree = Tree.InitaliseTree();
		Console.Title = "Engine";

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