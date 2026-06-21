using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading.Tasks;

namespace CsharpBooru.Views.Pages;

public partial class AboutView : UserControl {
    public AboutView()  => InitializeComponent();

	public void OpenGitHub (object sender, RoutedEventArgs args) => _ = OpenWebPage(this, "https://github.com/broumbroum/CsharpBooru");
	public void OpenAvalonia (object sender, RoutedEventArgs args) => _ = OpenWebPage(this, "https://avaloniaui.net/");
	public void OpenLibVLC (object sender, RoutedEventArgs args) => _ = OpenWebPage(this, "https://www.videolan.org/");
	public void OpenReactiveUI (object sender, RoutedEventArgs args) => _ = OpenWebPage(this, "https://reactiveui.net/");
	public void OpenSkiaSharp (object sender, RoutedEventArgs args) => _ = OpenWebPage(this, "https://mono.github.io/SkiaSharp/");
	public void OpenIcones8 (object sender, RoutedEventArgs args) => _ = OpenWebPage(this, "https://icones8.fr/");

	private static async Task OpenWebPage (Control control, string url) {
		var topLevel = TopLevel.GetTopLevel(control);

		if (!url.StartsWith("http")) url = "https://" + url;
		if (topLevel?.Launcher != null) await topLevel.Launcher.LaunchUriAsync(new Uri(url));
	}


}