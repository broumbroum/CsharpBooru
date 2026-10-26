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
	private TopLevel? topLevel;

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
		sizeButton.AttachedToVisualTree += (_, _) => {
			topLevel = TopLevel.GetTopLevel(sizeButton);
			if (topLevel != null)
				topLevel.PropertyChanged += OnTopLevelPropertyChanged;
		};
		sizeButton.DetachedFromVisualTree += (_, _) => {
			if (topLevel != null)
				topLevel.PropertyChanged -= OnTopLevelPropertyChanged;
			topLevel = null;
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
		double width = fullWidth;
		double height = fullHeight;

		if (showFullSize && topLevel != null && topLevel.ClientSize.Width > 0 && topLevel.ClientSize.Height > 0) {
			double scale = Math.Min(topLevel.ClientSize.Width / fullWidth, topLevel.ClientSize.Height / fullHeight);
			width *= scale;
			height *= scale;
		}

		applySize?.Invoke(showFullSize, width, height);
		if (sizeButton == null) return;

		sizeButton.Content = CreateIcon(showFullSize ? reduceIcon : enlargeIcon);
	}

	private void OnTopLevelPropertyChanged (object? sender, AvaloniaPropertyChangedEventArgs e) {
		if (e.Property == TopLevel.ClientSizeProperty && showFullSize)
			ApplySize();
	}

	private static Image CreateIcon (Bitmap icon) => new() {
		Source = icon,
		Width = 50,
		Height = 50
	};
}