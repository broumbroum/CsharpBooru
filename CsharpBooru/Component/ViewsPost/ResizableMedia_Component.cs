using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using Avalonia.Platform;
using System;

namespace CsharpBooru.Component.ViewsPost;
public abstract class ResizableMedia_Component {

	private const string IconPath = "avares://CsharpBooru/Resources/Icons/controls/";
	private static readonly Bitmap 
		enlargeIcon = new(AssetLoader.Open(new Uri(IconPath + "icons8-agrandir-100.png"))),
		reduceIcon = new(AssetLoader.Open(new Uri(IconPath + "icons8-réduire-100.png")));

	private Button? sizeButton;
	private Action<bool, double, double>? applySize;
	private int sizeLimit;
	private double fullWidth, fullHeight;
	private bool showFullSize;

	public const int size_max = 600;

	protected Button CreateResizeButton (int maxSize, Action<bool, double, double> applySize) {
		sizeLimit = maxSize;
		this.applySize = applySize;

		sizeButton = new Button() {
			Content = CreateIcon(enlargeIcon),
			Background = new SolidColorBrush(Color.FromRgb(239,239,239)),
			HorizontalAlignment = HorizontalAlignment.Center,
			Margin = new Thickness(0, 0, 0, 5),
			IsVisible = false,
			Height = 50,
			Width = 50
		};
		sizeButton.Click += (_, _) => {
			showFullSize = !showFullSize;
			ApplySize();
		};

		return sizeButton;
	}

	protected void SetMediaResolution (double width, double height) {
		fullWidth = width;
		fullHeight = height;

		bool canShowFullSize = width > sizeLimit || height > sizeLimit;
		if (!canShowFullSize)
			showFullSize = false;

		if (sizeButton != null)
			sizeButton.IsVisible = canShowFullSize;

		ApplySize();
	}

	protected void ResetMediaSize () {
		showFullSize = false;
		ApplySize();
	}

	private void ApplySize () {
		applySize?.Invoke(showFullSize, fullWidth, fullHeight);
		if (sizeButton == null) return;

		sizeButton.Content = CreateIcon(showFullSize ? reduceIcon : enlargeIcon);
	}

	private static Image CreateIcon (Bitmap icon) => new() {
		Source = icon,
		Width = 50,
		Height = 50
	};
}