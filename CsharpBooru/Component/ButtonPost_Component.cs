using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.IO;
using CsharpBooru.SQL;
using CsharpBooru.Setting;

namespace CsharpBooru.Component;
public static class ButtonPost_Component {

	public static Button Component (int index) {
		Post post = PostsManager.GetPost(index);

		Control cc = post switch {
			null => ErrorText("Invalid item"),
			_ => CreateThumbnailOrError(post)
		};

		Button button = new() {
			Content = cc,
			Width = 200,
			Height = 200,
			Margin = new Thickness(5),
			Background = Brushes.Transparent
		};

		return button;
	}

	private static Control CreateThumbnailOrError (Post post) {
		string thumbPath = ThumbnailsManager.GetThumbnailPath(post.Filename);

		if (!File.Exists(thumbPath))
			return ErrorText("Missing thumbnail");

		var img = new Image {
			Source = new Bitmap(thumbPath),
		};

		if(SettingValue.FillThumbnailPost == true)
			img.Stretch = Stretch.UniformToFill;
		else
			img.Stretch = Stretch.Uniform;

		return img;
	}

	private static TextBlock ErrorText (string message) => new() {
		Text = message,
		Foreground = Brushes.Red,
		HorizontalAlignment = HorizontalAlignment.Center,
		VerticalAlignment = VerticalAlignment.Center
	};
}
