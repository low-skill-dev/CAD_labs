﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary;

// https://stackoverflow.com/a/34801225/11325184
public class DirectBitmap : IDisposable
{
	public Bitmap Bitmap { get; private set; }
	public Int32[] Bits { get; private set; }
	public bool Disposed { get; private set; }
	public int Height { get; private set; }
	public int Width { get; private set; }

	protected GCHandle BitsHandle { get; private set; }

	public DirectBitmap(int width, int height)
	{
		Width = width;
		Height = height;
		Bits = new Int32[width * height];
		BitsHandle = GCHandle.Alloc(Bits, GCHandleType.Pinned);
		Bitmap = new Bitmap(width, height, width * 4, System.Drawing.Imaging.PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
	}

	public void SetPixel(int x, int y, Color colour)
	{
		int index = x + (y * Width);
		int col = colour.ToArgb();

		Bits[index] = col;
	}

	public Color GetPixel(int x, int y)
	{
		int index = x + (y * Width);
		int col = Bits[index];
		Color result = Color.FromArgb(col);

		return result;
	}

	public void Clear(Color color)
	{
		for(int i =0; i< Bits.Length; i++) {
			Bits[i] = color.ToArgb();
		}
	}

	public void Dispose()
	{
		if(Disposed) return;
		Disposed = true;
		Bitmap.Dispose();
		BitsHandle.Free();
	}
}