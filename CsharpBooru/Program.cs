using Avalonia;
using CsharpBooru.Setting;
using System;
using System.IO;

namespace CsharpBooru;

internal sealed class Program{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main (string[] args) {
		
		foreach (var arg in args) {
			if (arg.StartsWith("-setting=")) {
				string path = arg.Substring("-setting=".Length).Trim('"');
				SettingFile.configPath = Path.Combine(path, "setting.config");
			}
		}

		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
