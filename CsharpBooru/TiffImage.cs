using BitMiracle.LibTiff.Classic;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace CsharpBooru;
public class TiffImage {

	public static List<FileStream> LoadAllPages (string path) {
		var result = new List<FileStream>();

		using Tiff tiff = Tiff.Open(path, "r") ?? throw new Exception("Unable to open the .tiff/.tif file");

		int pageIndex = 0;

		do {
			int width = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
			int height = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

			int[] raster = new int[width * height];

			if (!tiff.ReadRGBAImage(width, height, raster))
				throw new Exception($"Unable to read page {pageIndex} of the .tiff/.tif file.");

			// Convert ARGB → RGBA
			byte[] pixels = new byte[width * height * 4];

			for (int i = 0; i < raster.Length; i++) {
				int pixel = raster[i];

				byte r = (byte)(pixel & 0xFF);         // RR
				byte g = (byte)((pixel >> 8) & 0xFF);  // GG
				byte b = (byte)((pixel >> 16) & 0xFF); // BB
				byte a = (byte)((pixel >> 24) & 0xFF); // AA

				int offset = i * 4;
				pixels[offset + 0] = r;
				pixels[offset + 1] = g;
				pixels[offset + 2] = b;
				pixels[offset + 3] = a;
			}


			// Create an empty SKBitmap
			var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
			var skBitmap = new SKBitmap(info);

			// Copy pixels → SKBitmap memory
			IntPtr ptr = skBitmap.GetPixels();
			Marshal.Copy(pixels, 0, ptr, pixels.Length);

			// Encode as PNG
			using var ms = new MemoryStream();
			using var image = SKImage.FromBitmap(skBitmap);
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);

			data.SaveTo(ms);
			ms.Position = 0;

			//Convert MemoryStream to FileStream
			var tempFilePath = Path.GetTempFileName();
			using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write)) {
				ms.CopyTo(fs);
			}

			result.Add(new FileStream(tempFilePath, FileMode.Open, FileAccess.Read));

			pageIndex++;

		} while (tiff.ReadDirectory());

		return result;
	}
}
