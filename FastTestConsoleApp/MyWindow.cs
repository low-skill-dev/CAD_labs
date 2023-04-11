using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FastTestConsoleApp;
public class MyWindow : GameWindow
{
	float[] vertices = {
		-0.5f, -0.5f, 0.0f, //Bottom-left vertex
		 0.5f, -0.5f, 0.0f, //Bottom-right vertex
		 0.0f,  0.5f, 0.0f  //Top vertex
	};

	int VertexBufferObject;
	int VertexArrayObject;
	Shader Shader;


	public MyWindow(int width, int height, string title = nameof(MyWindow))
		: base(GameWindowSettings.Default, new() { Size = new(width, height), Title = title })
	{
		Shader = new Shader("vert_shader", "frag_shader");
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
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		var bytes = Random.Shared.NextSingle();
		//GL.ClearColor(Random.Shared.NextSingle(), Random.Shared.NextSingle(), Random.Shared.NextSingle(), 1f);
		GL.Clear(ClearBufferMask.ColorBufferBit);

		Shader.Use();
		GL.BindVertexArray(VertexArrayObject);
		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

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
