using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using static System.MathF;

namespace FastTestConsoleApp;
public class MyWindow : GameWindow
{
	private readonly float[] vertices =
	  {
		  // positions        // colors
		  0.5f, -0.5f, 0.0f,  1.0f, 0.0f, 0.0f,   // bottom right
		 -0.5f, -0.5f, 0.0f,  0.0f, 1.0f, 0.0f,   // bottom left
		  0.0f,  0.5f, 0.0f,  0.0f, 0.0f, 1.0f    // top 
    };
	private readonly uint[] indices = {  // note that we start from 0!
		0, 1, 3,   // first triangle
		1, 2, 3    // second triangle
	};
	private int VertexBufferObject;
	private int VertexArrayObject;
	private int ElementBufferObject;
	private readonly Shader Shader;
	private readonly Stopwatch timer = new();


	public MyWindow(int width, int height, string title = nameof(MyWindow))
		: base(GameWindowSettings.Default, new() { Size = new(width, height), Title = title })
	{
		timer.Start();
		Shader = new Shader("vert.glsl", "frag.glsl");
	}

	protected override void OnKeyDown(KeyboardKeyEventArgs e)
	{
		base.OnKeyDown(e);

		if(e.Key == Keys.Escape) {
			Close();
		}
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		GL.ClearColor(System.Drawing.Color.Aqua);

		VertexBufferObject = GL.GenBuffer();

		GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


		var VAO = GL.GenVertexArray();
		VertexArrayObject = VAO;
		GL.BindVertexArray(VAO);

		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);

		GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(1);


		ElementBufferObject = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
		GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


		GL.GetInteger(GetPName.MaxVertexAttribs, out var nrAttributes);
		Console.WriteLine("Maximum number of vertex attributes supported: " + nrAttributes);

	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		GL.Clear(ClearBufferMask.ColorBufferBit);
		Shader.Use();

		var timeValue = (float)timer.Elapsed.TotalSeconds;
		var greenValue = Abs(Sin(timeValue));
		var vertexColorLocation = GL.GetUniformLocation(Shader.Handle, "ourColor");
		GL.Uniform4(vertexColorLocation, 0.0f, greenValue, 0.0f, 1.0f);


		GL.BindVertexArray(VertexArrayObject);
		GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

		SwapBuffers();
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);

		GL.Viewport(0, 0, e.Width, e.Height);
	}

	protected override void OnUnload()
	{
		base.OnUnload();

		Shader.Dispose();
	}
}
