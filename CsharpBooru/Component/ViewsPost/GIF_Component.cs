using Avalonia.Controls;
using Avalonia.Media.Imaging;
using System.IO;

namespace CsharpBooru.Component.ViewsPost;
public class GIF_Component : ResizableMedia_Component {

	private Bitmap? bmp;

	public StackPanel Component (ref string path) {
		bmp = new Bitmap(path);

		GifImage gif = new() {
			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
			MaxHeight = size_max,
			MaxWidth = size_max,

			Margin = new Avalonia.Thickness(0, 0, 10, 0)
		};

		gif.Load(path);

		StackPanel content = new();
		Button sizeButton = CreateResizeButton(size_max, (showFullSize, width, height) => {
			gif.MaxHeight = showFullSize ? height : size_max;
			gif.MaxWidth = showFullSize ? width : size_max;
		});
		SetMediaResolution(bmp.PixelSize.Width, bmp.PixelSize.Height);
		content.Children.Add(sizeButton);
		content.Children.Add(gif);

		return content;
	}

	public string GetInfo (ref string path) {
		if (bmp == null) return "";

		return "Dimension : " + new FileInfo(path).Length + "B \n"
			+ "Size : " + bmp.PixelSize.Width + " X " + bmp.PixelSize.Height
			;
	}
}
