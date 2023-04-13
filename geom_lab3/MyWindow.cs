using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.MathF;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Windows.Media;
using System.Windows.Forms;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using System.Windows.Media.Imaging;

namespace geom_lab3;
public class MyWindow : GameWindow
{
	private readonly float[] vertices;

	int VBO;
	int VAO;
	int EBO;
	Shader shader;
	Shader blackShader;
	[Obsolete]
	Shader lineShader;

	Stopwatch timer = new();

	public MyWindow(int width, int height, float[] vertices, string title = nameof(MyWindow))
		: base(GameWindowSettings.Default, new() { Size = new(width, height), Title = title })
	{
		timer.Start();
		shader = new Shader("vert.glsl", "frag.glsl");
		blackShader = new Shader("vert.glsl", "fragBlack.glsl");
		//lineShader = new Shader("vert.glsl", "line.glsl");
		this.vertices = vertices;


		//Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), (float)width / (float)height, 0.1f, 100.0f);
	}

	float xRotationD = 0;
	float yRotationD = 0;
	float zRotationD = 0;

	protected override void OnKeyDown(KeyboardKeyEventArgs e)
	{
		base.OnKeyDown(e);

		if(e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape) {
			Close();
		}

		if(e.Key == Keys.X || e.Key == Keys.Y || e.Key == Keys.Z) {
			if(e.Key == Keys.X) {
				xRotationD += 15f;
			}
			if(e.Key == Keys.Y) {
				yRotationD += 15f;
			}
			if(e.Key == Keys.Z) {
				zRotationD += 15f;
			}

			var rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(xRotationD));
			var rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yRotationD));
			var rotationZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(zRotationD));

			var rotation = rotationY * rotationX * rotationZ;
			GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "rotation"), true, ref rotation);
			GL.UniformMatrix4(GL.GetUniformLocation(blackShader.Handle, "rotation"), true, ref rotation);
			//GL.UniformMatrix4(GL.GetUniformLocation(lineShader.Handle, "rotation"), true, ref rotation);
		}

		if(e.Key == Keys.C) {
			var c = Random.Shared.Next() % 2 == 0 ? System.Drawing.Color.Red : System.Drawing.Color.Green;

			GL.Uniform4(GL.GetUniformLocation(shader.Handle, "ourColor"), new Vector4(c.R, c.G, c.B, 0.5f));
		}
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		GL.ClearColor(System.Drawing.Color.White);

		VBO = GL.GenBuffer();

		GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);


		this.VAO = GL.GenVertexArray();
		GL.BindVertexArray(VAO);

		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);

		//var c = System.Drawing.Color.Blue;
		//GL.Uniform4(GL.GetUniformLocation(shader.Handle, "ourColor"), new Vector4(c.R, c.G, c.B, 0.5f));
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		GL.Clear(ClearBufferMask.ColorBufferBit);

		GL.BindVertexArray(VAO);

		//lineShader.Use();
		//GL.DrawArrays(PrimitiveType.LineLoop, 1, vertices.Length);

		GL.Uniform4(GL.GetUniformLocation(shader.Handle, "ourColor"), new Vector4(0, 1, 0, 1));
		shader.Use();
		GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length);

		GL.Uniform4(GL.GetUniformLocation(shader.Handle, "ourColor"), new Vector4(0, 0, 0, 1));
		shader.Use();
		GL.DrawArrays(PrimitiveType.LineLoop, 0, vertices.Length);

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

		shader?.Dispose();
		lineShader?.Dispose();
		blackShader?.Dispose();
	}
}
