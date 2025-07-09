using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ZombieSurvival.Engine.Graphics;
using ZombieSurvival.Engine.NodeSystem;
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

		RunWindow();

		return 0;
	}
}