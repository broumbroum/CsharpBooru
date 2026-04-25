using System.IO;

namespace CsharpBooru.SQL;
internal static class FileManager {

	public static string SaveFile (string sourcePath) {
		string fileName = Path.GetFileName(sourcePath);
		string name = Path.GetFileNameWithoutExtension(fileName);
		string extension = Path.GetExtension(fileName);

		System.Diagnostics.Debug.WriteLine("Path.GetDirectoryName: " + Path.GetDirectoryName(sourcePath) + " | DataBase.FilesPath: " + DataBase.FilesPath);
		if (Path.GetDirectoryName(sourcePath) + "\\" == Path.Combine(DataBase.FilesPath)) return fileName;

		string destinationPath = Path.Combine(DataBase.FilesPath, fileName);

		if (File.Exists(destinationPath) == true) {
			int counter = 1;
			string newFileName = name;
			do {
				newFileName = $"{name} ({counter})";
				destinationPath = Path.Combine(DataBase.FilesPath, newFileName + extension);
				counter++;
			} while (File.Exists(destinationPath));
			name = newFileName;
		}

		File.Copy(sourcePath, destinationPath);
		ThumbnailsManager.CreateThumbnails(destinationPath, DataBase.ThumbnailsPath + name + extension + ".jpg");
		return name + extension;
	}

	public static void DeleteFile (string filename) {
		if (PostsManager.FilenameExists(filename) == false) {
			string file = Path.Combine(DataBase.FilesPath, filename);
			File.Delete(file);
			ThumbnailsManager.DeleteThumbnail(file);
		}
	}


}
