﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;

namespace geom_lab3;
public class Shader : IDisposable
{
	public int Handle;

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

		GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out var success);
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

	private float rC => (Random.Shared.NextSingle() / 2) + 0.25f;
	public void Use()
	{
		GL.UseProgram(Handle);
	}

	public int GetAttribLocation(string attribName)
	{
		return GL.GetAttribLocation(Handle, attribName);
	}
}
