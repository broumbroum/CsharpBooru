using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CsharpBooru.Setting;
using CsharpBooru.SQL;
using System.IO;

namespace CsharpBooru.Views; 
public partial class MainWindow : Window {
	public MainWindow () {

		InitializeComponent();

		if (SettingFile.Load() == false) {
			FirstStart.IsVisible = true;
			Start.IsVisible = false;
		} else {
			FirstStart.IsVisible = false;
			Start.IsVisible = true;

			DataBase.path = Path.Combine(SettingValue.Path);
		}
	}

	public async void SelectFolder (object sender, RoutedEventArgs args) {
		string path;
		var topLevel = TopLevel.GetTopLevel(this);

		var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions {
			Title = "Select a Folder",
			AllowMultiple = false,
		});

		if (folders.Count == 0)
			return;

		path = folders[0].Path.LocalPath;
		DataBase.path = path;

		if (Directory.Exists(DataBase.FilesPath) == false) Directory.CreateDirectory(DataBase.FilesPath);
		if (Directory.Exists(DataBase.ThumbnailsPath) == false) Directory.CreateDirectory(DataBase.ThumbnailsPath);
		if (File.Exists(Path.Combine(path , "Booru.db")) == false) {
			DataBase.CreateSQL();
		}

		SettingFile.Create(path);

		FirstStart.IsVisible = false;
		Start.IsVisible = true;

	}
}