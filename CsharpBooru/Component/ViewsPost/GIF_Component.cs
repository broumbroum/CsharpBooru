using Avalonia.Media.Imaging;
using System.IO;

namespace CsharpBooru.Component.ViewsPost;
public class GIF_Component {

	private Bitmap? bmp;

	public GifImage Component (ref string path) {
		bmp = new Bitmap(path);

		GifImage gif = new() {
			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			MaxHeight = 700,
			MaxWidth = 700,

			Margin = new Avalonia.Thickness(0, 0, 10, 0)
		};

		gif.Load(path);

		return gif;
	}

	public string GetInfo (ref string path) {
		if (bmp == null) return "";

		return "Dimension : " + new FileInfo(path).Length + "B \n"
			+ "Size : " + bmp.PixelSize.Width + " X " + bmp.PixelSize.Height
			;
	}
}
