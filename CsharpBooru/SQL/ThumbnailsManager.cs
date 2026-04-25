using System.Threading.Tasks;
using LibVLCSharp.Shared;
using System.IO;
using SkiaSharp;

namespace CsharpBooru.SQL;
internal static class ThumbnailsManager {
	public static void CreateThumbnails (string inputPath, string outputPath) {
		switch (Path.GetExtension(inputPath).ToLower()) {
			case ".png" or ".jpg" or ".jpeg" or ".gif" or ".ico" or ".webp" or ".tiff" or ".tif":
			PictureThumbnails(inputPath, outputPath);
			break;
			case ".mp4" or ".avi" or ".webm" or ".mkv":
			_ = VideoThumbnails(inputPath, outputPath);
			break;
		}
	}

	public static void RegenerateThumbnails (string filename) {
		if (File.Exists(DataBase.ThumbnailsPath + filename + ".jpg")) {
			DeleteThumbnail(filename);
		}
		CreateThumbnails(DataBase.FilesPath + filename, DataBase.ThumbnailsPath + filename + ".jpg");
	}

	public static void RegenerateThumbnails (int id) {
		Post post = PostsManager.GetPost(id);
		if (post == null) return;
		RegenerateThumbnails(post.Filename);
	}

	private static void PictureThumbnails (string inputPath, string outputPath) {
		using var input = File.OpenRead(inputPath);
		using var bitmap = SKBitmap.Decode(input);

		if (bitmap == null)
			return;

		Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

		using var image = SKImage.FromBitmap(bitmap);
		using var data = image.Encode(SKEncodedImageFormat.Jpeg, 80);

		using var output = File.OpenWrite(outputPath);
		data.SaveTo(output);
	}

	private static async Task VideoThumbnails (string inputPath, string outputPath) {
		Core.Initialize();

		var libVLC = new LibVLC("--vout=dummy");
		using var media = new Media(libVLC, inputPath, FromType.FromPath);
		using var mp = new MediaPlayer(media);

		//mp.Volume = 0;
		mp.Mute = true;

		var tcs = new TaskCompletionSource<bool>();

		// Event triggered when the first image is displayed
		mp.TimeChanged += (sender, e) => {
			if (e.Time > 0)
				tcs.TrySetResult(true);
		};

		mp.Play();

		//Wait for the first frame to be decoded.
		await tcs.Task;

		// Take a snapshot
		Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
		mp.TakeSnapshot(0, outputPath, 0, 0);

		mp.Stop();
	}

	public static string GetThumbnailPath (string filename) {
		return Path.Combine(DataBase.ThumbnailsPath + filename + ".jpg");
	}

	public static void DeleteThumbnail (string filename) {
		string thumbnail = GetThumbnailPath(filename + ".jpg");
		File.Delete(thumbnail);
	}
}
