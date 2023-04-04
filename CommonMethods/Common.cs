using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using System.IO;


public class Common
{
	// https://stackoverflow.com/a/22501616/11325184
	public static BitmapImage BitmapToImageSource(Bitmap bitmap)
	{
		using(MemoryStream memory = new MemoryStream()) {
			bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
			memory.Position = 0;
			BitmapImage bitmapimage = new BitmapImage();
			bitmapimage.BeginInit();
			bitmapimage.StreamSource = memory;
			bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapimage.EndInit();

			return bitmapimage;
		}
	}
}
