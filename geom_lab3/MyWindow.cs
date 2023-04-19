using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;
using System.Linq;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

namespace geom_lab3;
public class MyWindow : GameWindow
{
	private static float[] CalculateBarycentric(int verticesCount)
	{
		var n = verticesCount / 3 / 3; // 3 points of 3 coordinates
		var result = new List<float>(n);

		for(var i = 0; i < n; i++) {
			result.AddRange(new float[] {
				1, 0, 0,
				0, 1, 0,
				0, 0, 1 });
		}

		return result.ToArray();
	}
	private static float[] ConcatVertsToBaries(float[] first, float[] second)
	{
		var result = new List<float>();

		for(var i = 0; i < first.Length / 3; i++) {
			result.AddRange(first.Skip(3 * i).Take(3));
			result.AddRange(second.Skip(3 * i).Take(3));
		}

		return result.ToArray();
	}


	private Matrix4 perspectiveMatrix;
	private float xRotationD = 0;
	private float yRotationD = 0;
	private float zRotationD = 0;
	private float scale = 1;
	private bool borderMode = false;
	private bool useGrayPolys = false;

	private readonly float[] VertsAndBaries;
	private int VBO;
	private int VAO;
	private readonly Shader shader;

	public MyWindow(int width, int height, float[] vertices, string title = nameof(MyWindow))
		: base(GameWindowSettings.Default, new() { Size = new(width, height), Title = title })
	{
		shader = new Shader("vert.glsl", "frag.glsl");

		VertsAndBaries = ConcatVertsToBaries(vertices, CalculateBarycentric(vertices.Length));
	}

	protected override void OnLoad()
	{
		base.OnLoad();

		VBO = GL.GenBuffer();
		GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
		GL.BufferData(BufferTarget.ArrayBuffer, VertsAndBaries.Length * sizeof(float), VertsAndBaries, BufferUsageHint.StaticDraw);

		VAO = GL.GenVertexArray();
		GL.BindVertexArray(VAO);

		// 3 points and 3 barycentrices = stride is 6 * sizeof(float)
		GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
		GL.EnableVertexAttribArray(0);

		GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
		GL.EnableVertexAttribArray(1);


		GL.ClearColor(System.Drawing.Color.White);
		GL.Enable(EnableCap.DepthTest);
		GL.DepthFunc(DepthFunction.Less);
		GL.DepthMask(true);
		SetAllUniforms();
	}

	private void SetAllUniforms()
	{
		/* uniform int allBlack;
		 * uniform int grayPolygons;
		 * uniform mat4 rotate;
		 * uniform mat4 scale;
		 */
		var rotationX = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(xRotationD));
		var rotationY = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(yRotationD));
		var rotationZ = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(zRotationD));
		var rotation = rotationY * rotationX * rotationZ;

		var scale = Matrix4.CreateScale(this.scale);

		GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "rotate"), false, ref rotation);
		GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, "scale"), false, ref scale);
		GL.Uniform1(GL.GetUniformLocation(shader.Handle, "allBlack"), borderMode ? 1 : 0);
		GL.Uniform1(GL.GetUniformLocation(shader.Handle, "grayPolygons"), useGrayPolys ? 1 : 0);
	}

	protected override void OnRenderFrame(FrameEventArgs args)
	{
		base.OnRenderFrame(args);

		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

		shader.Use();
		GL.DrawArrays(borderMode ? PrimitiveType.LineLoop : PrimitiveType.Triangles, 0, VertsAndBaries.Length);

		SwapBuffers();
	}

	protected override void OnKeyDown(KeyboardKeyEventArgs e)
	{
		base.OnKeyDown(e);

		if(e.Key == Keys.Escape) {
			Close();
		}

		if(e.Key == Keys.X || e.Key == Keys.Y || e.Key == Keys.Z) {
			if(e.Key == Keys.X) {
				xRotationD += e.Modifiers == KeyModifiers.Alt ? -15f : 15f;
			}
			if(e.Key == Keys.Y) {
				yRotationD += e.Modifiers == KeyModifiers.Alt ? -15f : 15f;
			}
			if(e.Key == Keys.Z) {
				zRotationD += e.Modifiers == KeyModifiers.Alt ? -15f : 15f;
			}
		}

		if(e.Key == Keys.Equal || e.Key == Keys.Minus) {
			if(e.Key == Keys.Equal) {
				scale += 0.1f;
			}
			if(e.Key == Keys.Minus) {
				if(scale > 0.1) {
					scale -= 0.09f;
				}
			}
		}

		if(e.Key == Keys.B) {
			borderMode = !borderMode;
		}

		if(e.Key == Keys.G) {
			useGrayPolys = !useGrayPolys;
		}

		SetAllUniforms();
	}

	protected override void OnResize(ResizeEventArgs e)
	{
		base.OnResize(e);
		GL.Viewport(0, 0, e.Width, e.Height);
	}
	protected override void OnUnload()
	{
		base.OnUnload();
		shader.Dispose();
	}
}
