using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace CsharpBooru.Component.ViewsPost;
public class Image_Component {

	 private Bitmap? bmp;

	public Image Component (ref string path) {
		bmp = new Bitmap(path);

		Image image = new() {
			Source = bmp,

			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,

			MaxHeight = 700,
			MaxWidth = 700,
			Margin = new Thickness(0, 0, 10, 0)
		};

		return image;
	}

	public string GetInfo (ref string path) {
		if (bmp == null) return "";

		return "Dimension : " + new FileInfo(path).Length + "B \n"
			+ "Size : " + bmp.PixelSize.Width + " X " + bmp.PixelSize.Height
			;
	}
}
