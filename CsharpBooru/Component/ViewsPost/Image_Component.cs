using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;

namespace CsharpBooru.Component.ViewsPost;
public class Image_Component : ResizableMedia_Component {

	private Bitmap? bmp;

	public StackPanel Component (ref string path) {
		bmp = new Bitmap(path);

		Image image = new() {
			Source = bmp,

			HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,

			MaxHeight = size_max,
			MaxWidth = size_max,
			Margin = new Thickness(0, 0, 10, 0)
		};

		StackPanel content = new();
		Button sizeButton = CreateResizeButton(size_max, (showFullSize, width, height) => {
			image.MaxHeight = showFullSize ? height : size_max;
			image.MaxWidth = showFullSize ? width : size_max;
		});
		SetMediaResolution(bmp.PixelSize.Width, bmp.PixelSize.Height);
		content.Children.Add(sizeButton);
		content.Children.Add(image);

		return content;
	}

	public string GetInfo (ref string path) {
		if (bmp == null) return "";

		return "Dimension : " + new FileInfo(path).Length + "B \n"
			+ "Size : " + bmp.PixelSize.Width + " X " + bmp.PixelSize.Height
			;
	}
}
