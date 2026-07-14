using Avalonia.Controls;
using Avalonia.Media.Imaging;
using SkiaSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CsharpBooru;
public class GifImage : Image {
	private List<GifFrame>? _frames;
	//private bool _isPlaying;

	public async void Load (string path) {
		_frames = DecodeGif(path);

		if (_frames.Count == 0)
			return;

		//_isPlaying = true;
		await PlayAnimation();
	}

	private static List<GifFrame> DecodeGif (string path) {
		var frames = new List<GifFrame>();

		using var stream = File.OpenRead(path);
		using var codec = SKCodec.Create(stream);

		var frameInfos = codec.FrameInfo;
		var frameCount = codec.FrameCount;

		for (int i = 0; i < frameCount; i++) {
			var info = codec.Info;
			var bitmap = new SKBitmap(info.Width, info.Height);

			var opts = new SKCodecOptions(i);
			codec.GetPixels(bitmap.Info, bitmap.GetPixels(), opts);

			using var ms = new MemoryStream();
			using var image = SKImage.FromBitmap(bitmap);
			using var data = image.Encode(SKEncodedImageFormat.Png, 100);
			data.SaveTo(ms);
			ms.Position = 0;

			frames.Add(new GifFrame {
				Bitmap = new Bitmap(ms),
				DelayMs = frameInfos[i].Duration
			});
		}

		return frames;
	}

	private async Task PlayAnimation () {
		if (_frames == null || _frames.Count == 0) return;

		while (true /*_isPlaying*/) {
			foreach (var frame in _frames) {
				Source = frame.Bitmap;
				await Task.Delay(frame.DelayMs);
			}
		}
	}

	//public void Stop () => _isPlaying = false;
}

public class GifFrame {
	public required Bitmap Bitmap { get; set; }
	public int DelayMs { get; set; }
}