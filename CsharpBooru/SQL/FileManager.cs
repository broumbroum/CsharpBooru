using System.IO;

namespace CsharpBooru.SQL;
internal static class FileManager {

	public static string SaveFile (string sourcePath) {
		string fileName = Path.GetFileName(sourcePath); //(ex: "image.png")
		string name = Path.GetFileNameWithoutExtension(fileName); //(ex: "image")
		string extension = Path.GetExtension(fileName); //(ex: "png")

		//Avoid copying a file already in the database
		if (Path.GetDirectoryName(sourcePath) == Path.GetDirectoryName(DataBase.FilesPath)) return fileName;

		string destinationPath = Path.Combine(DataBase.FilesPath, fileName);

		//If a file with the same name already exists → create a unique name
		if (File.Exists(destinationPath) == true) {
			int counter = 1;
			string newFileName = name;
			do {
				newFileName = $"{name} ({counter})";
				destinationPath = Path.Combine(DataBase.FilesPath, newFileName + extension);
				counter++;
			} while (File.Exists(destinationPath));
			//Updates the final name
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
