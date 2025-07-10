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

		ElementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);

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

		ArgumentNullException.ThrowIfNull(CurrentCamera, nameof(CurrentCamera));

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		GL.BindVertexArray(VertexArrayObject);

		foreach (Node node in Tree.GetTree().GetAllNodes())
		{
			if (node is not MeshContainer container)
			{
				continue;
			}


			if (container.Mesh is null)
			{
				continue;
			}

			Mesh mesh = container.Mesh;

			float[] feed = mesh.IntoFeed(Vector3.Zero, container.GlobalScale);

			GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.Indices.Length * sizeof(uint), mesh.Indices, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ArrayBuffer, feed.Length * sizeof(float), feed, BufferUsageHint.StaticDraw);

			// TODO - Load Custom Textures
			MTexture0.Use(TextureUnit.Texture0);
			MTexture1.Use(TextureUnit.Texture1);
			Shader.Use();

			Matrix4 model = Matrix4.CreateTranslation((GLVector3)container.Position)
			* Matrix4.CreateFromQuaternion(container.GlobalQuaternion);

			Shader.SetMatrix4("model", model);
			Shader.SetMatrix4("view", CurrentCamera.GetViewMatrix());
			Shader.SetMatrix4("projection", CurrentCamera.GetProjectionMatrix());

			GL.DrawElements(mesh.PrimitiveType, mesh.Indices.Length, DrawElementsType.UnsignedInt, 0);
		}

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

		Tree.GetTree().UpdateAllNodes(e.Time);
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