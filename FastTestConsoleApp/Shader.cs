using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FastTestConsoleApp;
public class Shader : IDisposable
{
	int Handle;

	public Shader(string vertexPath, string fragmentPath)
	{
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

		GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
		if(success == 0) {
			Console.WriteLine(
				GL.GetShaderInfoLog(vertexShader));
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
		// TODO: is this needed ?
		//GC.SuppressFinalize(this);
	}
	~Shader()
	{
		if(!IsDisposed) {
			Console.WriteLine("GPU Resource leak! Did you forget to call Dispose()?");
		}
	}

	public void Use()
	{
		GL.UseProgram(Handle);
	}
}
