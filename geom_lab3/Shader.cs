using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace geom_lab3;
public class Shader : IDisposable
{
	Stopwatch timer = new();

	public int Handle;

	public Shader(string vertexPath, string fragmentPath)
	{
		timer.Start();

		var vertexShader = GL.CreateShader(ShaderType.VertexShader);
		var vertexShaderSrc = File.ReadAllText(vertexPath);
		GL.ShaderSource(vertexShader, vertexShaderSrc);

		var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
		var fragmentShaderSrc = File.ReadAllText(fragmentPath);
		GL.ShaderSource(fragmentShader, fragmentShaderSrc);

		GL.CompileShader(vertexShader);
		GL.CompileShader(fragmentShader);

		GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int success);
		if(success == 0) {
			Console.WriteLine(
				GL.GetShaderInfoLog(vertexShader));
		}

		GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
		if(success == 0) {
			Console.WriteLine(
				GL.GetShaderInfoLog(fragmentShader));
		}

		Handle = GL.CreateProgram();
		GL.AttachShader(Handle, vertexShader);
		GL.AttachShader(Handle, fragmentShader);

		GL.LinkProgram(Handle);


		GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
		if(success == 0) {
			Console.WriteLine(
				GL.GetProgramInfoLog(Handle));
		}

		GL.DetachShader(Handle, vertexShader);
		GL.DetachShader(Handle, fragmentShader);
		GL.DeleteShader(vertexShader);
		GL.DeleteShader(fragmentShader);
	}

	public bool IsDisposed;
	public void Dispose()
	{
		if(!IsDisposed) {
			IsDisposed = true;
			GL.DeleteProgram(Handle);
		}
	}
	~Shader()
	{
		if(!IsDisposed) {
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
		}
	}

	private float rC => Random.Shared.NextSingle() / 2 + 0.25f;
	public void Use()
	{
		//double timeValue = timer.Elapsed.TotalSeconds;
		//float greenValue = (float)Math.Sin(timeValue) / 2.0f + 0.5f;
		//int vertexColorLocation = GL.GetUniformLocation(Handle, "ourColor");
		//var color = new Vector4(rC, rC, rC, rC);
		//GL.Uniform4(vertexColorLocation, color);
		GL.UseProgram(Handle);
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(Handle, attribName);
	}
}
