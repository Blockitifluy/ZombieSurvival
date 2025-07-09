global using GLVector3 = OpenTK.Mathematics.Vector3;
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ZombieSurvival.Engine.NodeSystem;
using ZombieSurvival.Nodes;

namespace ZombieSurvival.Engine.Graphics;

public class Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : GameWindow(gameWindowSettings, nativeWindowSettings)
{
	// In NDC, (0, 0) is the center of the screen.
	// Negative X coordinates move to the left, positive X move to the right.
	// Negative Y coordinates move to the bottom, positive Y move to the top.
	// OpenGL only supports rendering in 3D, so to create a flat triangle, the Z coordinate will be kept as 0.
	private readonly float[] Vertices = [
	 	// Position         Texture coordinates
             0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left
	];

	private readonly uint[] Indices = [
		0, 1, 3,
		1, 2, 3
	];

	int ElementBufferObject;
	int VertexBufferObject;
	int VertexArrayObject;

	[AllowNull]
	private Shader Shader;
	[AllowNull]
	private Texture MTexture0;
	[AllowNull]
	private Texture MTexture1;

	private static Camera? CurrentCamera => Camera.CurrentCamera;
	private double Time;

	protected override void OnLoad()
	{
		base.OnLoad();

		ArgumentNullException.ThrowIfNull(CurrentCamera, nameof(CurrentCamera));

		GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

		GL.Enable(EnableCap.DepthTest);

		VertexArrayObject = GL.GenVertexArray();
		GL.BindVertexArray(VertexArrayObject);

		VertexBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);

		ElementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(uint), Indices, BufferUsageHint.StaticDraw);

		Shader = new Shader("shaders/shader.vert", "shaders/shader.frag");
		Shader.Use();

		var vertexLocation = Shader.GetAttribLocation("aPosition");
		GL.EnableVertexAttribArray(vertexLocation);
		GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

		var texCoordLocation = Shader.GetAttribLocation("aTexCoord");
		GL.EnableVertexAttribArray(texCoordLocation);
		GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

		MTexture0 = Texture.LoadFromFile("resources/container.png");
		MTexture0.Use(TextureUnit.Texture0);

		MTexture1 = Texture.LoadFromFile("resources/awesome.png");
		MTexture1.Use(TextureUnit.Texture1);

		Shader.SetInt("texture0", 0);
		Shader.SetInt("texture1", 1);

		CurrentCamera.Position = Vector3.Forward * 3;
		CurrentCamera.AspectRatio = Size.X / (float)Size.Y;

		CursorState = CursorState.Grabbed;
	}

	protected override void OnRenderFrame(FrameEventArgs e)
	{
		base.OnRenderFrame(e);

		Time += 4.0 * e.Time;

		ArgumentNullException.ThrowIfNull(CurrentCamera, nameof(CurrentCamera));

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		GL.BindVertexArray(VertexArrayObject);

		MTexture0.Use(TextureUnit.Texture0);
		MTexture1.Use(TextureUnit.Texture1);
		Shader.Use();

		var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(Time));
		Shader.SetMatrix4("model", model);
		Shader.SetMatrix4("view", CurrentCamera.GetViewMatrix());
		Shader.SetMatrix4("projection", CurrentCamera.GetProjectionMatrix());

		GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);

		SwapBuffers();
	}

	protected override void OnUpdateFrame(FrameEventArgs e)
	{
		base.OnUpdateFrame(e);

		if (!IsFocused) // Check to see if the window is focused
		{
			return;
		}

		var input = KeyboardState;
		Input.KeyboardState = input;

		if (input.IsKeyDown(Keys.Escape))
		{
			Close();
		}

		if (input.IsKeyPressed(Keys.F11))
		{
			WindowState state;
			if (WindowState == WindowState.Fullscreen)
			{
				state = WindowState.Normal;
			}
			else
			{
				state = WindowState.Fullscreen;
			}
			WindowState = state;
		}

		Input.KeyboardState = KeyboardState;
		Input.MouseState = MouseState;

		List<Node> nodes = Tree.GetTree().GetAllNodes();
		foreach (Node node in nodes)
		{
			node.Update(e.Time);
		}
	}

	protected override void OnKeyDown(KeyboardKeyEventArgs e)
	{
		base.OnKeyDown(e);

		Input.SendKeyDown(this, e);
	}

	protected override void OnKeyUp(KeyboardKeyEventArgs e)
	{
		base.OnKeyUp(e);

		Input.SendKeyUp(this, e);
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		ArgumentNullException.ThrowIfNull(CurrentCamera, nameof(CurrentCamera));

		CurrentCamera.Fov -= e.OffsetY;
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);

		ArgumentNullException.ThrowIfNull(CurrentCamera, nameof(CurrentCamera));

		GL.Viewport(0, 0, Size.X, Size.Y);

		CurrentCamera.AspectRatio = Size.X / (float)Size.Y;
	}
}