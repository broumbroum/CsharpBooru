using Avalonia;
using CsharpBooru.Setting;
using CsharpBooru.SQL;
using System;
using System.IO;

namespace CsharpBooru;

internal sealed class Program{
	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main (string[] args) {
		bool runMcpServer = false;

		foreach (var arg in args) {
			if (arg.StartsWith("-setting=")) {
				string path = arg.Substring("-setting=".Length).Trim('"');
				SettingFile.configPath = Path.Combine(path, "setting.config");
			} else if (arg == "-mcp") {
				runMcpServer = true;
			}
		}

		if (runMcpServer) {
			StartMcpServer();
			return;
		}

		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	private static void StartMcpServer () {
		if (!SettingFile.Load())
			throw new InvalidOperationException("C#Booru must be configured once before starting its MCP server.");

		if (!SettingFile._values.TryGetValue("Path", out string? databaseDirectory) ||
			string.IsNullOrWhiteSpace(databaseDirectory))
			throw new InvalidOperationException("The C#Booru database folder is missing from setting.config.");

		DataBase.path = databaseDirectory;
		if (!File.Exists(DataBase.PathBooru))
			throw new FileNotFoundException("The C#Booru database was not found.", DataBase.PathBooru);

		if (!Directory.Exists(DataBase.FilesPath) || !Directory.Exists(DataBase.ThumbnailsPath))
			throw new DirectoryNotFoundException("The C#Booru files or thumbnails folder is missing.");

		DataBase.DataBaseCheck();
		MCP.McpServer.RunAsync().GetAwaiter().GetResult();
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp()
		=> AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.WithInterFont()
			.LogToTrace();
}
