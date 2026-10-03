using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using System.IO;

namespace CsharpBooru.Component.ViewsPost;
public class Tiff_Component : ResizableMedia_Component {

	private int currentPage = 0;
	private List<FileStream>? pages;
	private TextBlock? pageIndicator;

	public StackPanel Component (ref string path) {
		pages = TiffImage.LoadAllPages(path);


		StackPanel root = new() {
			Orientation = Orientation.Vertical,
			HorizontalAlignment = HorizontalAlignment.Center
		};

		if (pages.Count == 0) {
			TextBox errorText = new() {
				Text = "No pages found in the .tiff/.tif",
				Foreground = new SolidColorBrush(Colors.Red)
			};
			root.Children.Add(errorText);
			return root;
		}

		Button sizeButton = CreateResizeButton(size_max, (showFullSize, width, height) => {
			imageControl.MaxHeight = showFullSize ? height : size_max;
			imageControl.MaxWidth = showFullSize ? width : size_max;
		});
		root.Children.Add(sizeButton);

		//Image
		DisplayCurrentPage();
		root.Children.Add(imageControl);

		//Navigation
		StackPanel navPanel = CreatNavigation();
		root.Children.Add(navPanel);

		return root;
	}

	public string GetInfo (ref string path) {
		if (pages == null || pages.Count == 0)
			return "No pages found in the .tiff/.tif";

		string str = "Size : " + new FileInfo(path).Length + "B \n";

		for (int i = 0; i < pages.Count; i++) {
			pages[i].Position = 0;
			Bitmap bmp = new(pages[i]);

			str += "Page " + (i + 1) + " : \n";
			str += "    Dimension : " + bmp.PixelSize.Width + " X " + bmp.PixelSize.Height + "\n";
		}

		return str;
	}

	private StackPanel CreatNavigation () {
		if (pages == null || pages.Count <= 1)
			return new StackPanel();

		StackPanel navPanel = new() {
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Center,
			Spacing = 10,
			Margin = new Thickness(0, 10, 0, 0)
		};

		Button btnPrev = CreateButton("← Previous");
		Button btnNext = CreateButton("Next →");

		btnPrev.Click += (_, __) => {
			if (currentPage > 0) {
				currentPage--;
				ResetMediaSize();
				DisplayCurrentPage();
			}
		};

		btnNext.Click += (_, __) => {
			if (currentPage < pages.Count - 1) {
				currentPage++;
				ResetMediaSize();
				DisplayCurrentPage();
			}
		};

		pageIndicator = new TextBlock() {
			Text = $"Page {currentPage + 1} / {pages.Count}",
			VerticalAlignment = VerticalAlignment.Center,
			Margin = new Thickness(10, 0, 10, 0)
		};

		navPanel.Children.Add(btnPrev);
		navPanel.Children.Add(pageIndicator);
		navPanel.Children.Add(btnNext);

		return navPanel;
	}

	private void DisplayCurrentPage () {
		if (pages == null || currentPage >= pages.Count) return;

		pages[currentPage].Position = 0;
		Bitmap currentBitmap = new(pages[currentPage]);
		imageControl.Source = currentBitmap;
		SetMediaResolution(currentBitmap.PixelSize.Width, currentBitmap.PixelSize.Height);

		if (pageIndicator != null)
			pageIndicator.Text = $"Page {currentPage + 1} / {pages.Count}";
	}

	private readonly Image imageControl = new() {
		Stretch = Stretch.Uniform,
		MaxWidth = size_max,
		MaxHeight = size_max
	};

	private static Button CreateButton (string txt) => new() {
		Content = txt,
		Width = 120,
		HorizontalAlignment = HorizontalAlignment.Center
	};

}
