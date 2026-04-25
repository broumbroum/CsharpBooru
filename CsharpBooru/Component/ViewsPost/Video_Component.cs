using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using LibVLCSharp.Avalonia;
using LibVLCSharp.Shared;
using System;
using System.IO;

namespace CsharpBooru.Component.ViewsPost;
public class Video_Component : IDisposable {

	private static readonly LibVLC libVLC = new();
	private MediaPlayer? mediaPlayer;
	private Media? media;

	private const int 
		MaxVideoSize = 600, 
		SizeIconButton = 50;

	public Control Component (ref string path) {
		mediaPlayer = new MediaPlayer(libVLC);
		media = new Media(libVLC, path, FromType.FromPath);

		CreateVideoView();
		CreatePlayPauseButton();
		CreateSeekBar();
		CreateVolumeBar();
		CreateVolumeIcon();

		AttachSeekEvents();
		AttachVideoLoadedEvent();
		AttachPlayPauseEvent();
		AttachVolumeEvent();

		StackPanel root = BuildRoot(BuildControls());

		return root;
	}

	public string GetInfo (ref string path) {
		if (media == null) return "";
		return "Size : " + new FileInfo(path).Length + "B \n";
	}

	private static Image Play (bool isPlay) {
		string pathIcon = isPlay
			? "avares://CsharpBooru/Resources/Icons/media_controls/icons8-pause-100.png"
			: "avares://CsharpBooru/Resources/Icons/media_controls/icons8-play-100.png";
		Bitmap btm = new(AssetLoader.Open(new Uri(pathIcon)));

		return new() {
			Source = btm,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			Width = SizeIconButton,
			Height = SizeIconButton
		};
	}

	#region Controls Video UI
	private VideoView? videoView;
	private Button playPauseButton;
	private Slider seekBar;
	private Slider volumeBar;
	private Image volumeIcon;

	private VideoView CreateVideoView () => videoView = new VideoView {
		HorizontalAlignment = HorizontalAlignment.Center,
		Width = MaxVideoSize,
		Height = MaxVideoSize,
		MaxWidth = MaxVideoSize,
		MaxHeight = MaxVideoSize,
		Margin = new Thickness(0, 0, 10, 0),
		MediaPlayer = mediaPlayer
	};

	private Button CreatePlayPauseButton () => playPauseButton = new Button {
		Content = Play(true),
		Width = SizeIconButton + 5,
		Height = SizeIconButton + 5
	};

	private Slider CreateSeekBar () => seekBar = new Slider {
		Minimum = 0,
		Maximum = 1000,
		Value = 0,
		Width = 600,
		VerticalAlignment = VerticalAlignment.Center
	};

	private Slider CreateVolumeBar () => volumeBar = new Slider {
		Minimum = 0,
		Maximum = 100,
		Value = 100,
		Width = 100
	};

	private Image CreateVolumeIcon () => volumeIcon = new Image {
		Source = new Bitmap(AssetLoader.Open(new Uri("avares://CsharpBooru/Resources/Icons/media_controls/icons8-audio-100.png"))),
		Width = SizeIconButton,
		Height = SizeIconButton,
		Margin = new Thickness(0, 0, 0, 10)
	};

	private StackPanel BuildControls () => new StackPanel {
		//Background = new SolidColorBrush(new Color(128,128, 255,255)), //DEBUG
		Orientation = Orientation.Horizontal,
		Spacing = 10,
		HorizontalAlignment = HorizontalAlignment.Center,
		Children = { playPauseButton, seekBar, volumeIcon, volumeBar }
	};

	private StackPanel BuildRoot (Control controls) => new StackPanel {
		Orientation = Orientation.Vertical,
		Spacing = 10,
		Children = { videoView, controls }
	};
	#endregion

	#region Control Video Event
	private void AttachSeekEvents () {
		if (seekBar == null || mediaPlayer == null) return;

		bool userDragging = false;
		bool updatingFromCode = false;

		seekBar.AddHandler(InputElement.PointerPressedEvent,
			(_, _) => userDragging = true, RoutingStrategies.Tunnel);

		seekBar.AddHandler(InputElement.PointerReleasedEvent,
			(_, _) => userDragging = false, RoutingStrategies.Tunnel);

		seekBar.PropertyChanged += (_, e) =>
		{
			if (e.Property == Slider.ValueProperty &&
				userDragging && !updatingFromCode &&
				mediaPlayer.Length > 0) {
				mediaPlayer.Position = (float)(seekBar.Value / 1000.0);
			}
		};
	}

	private void AttachVideoLoadedEvent () => videoView.Loaded += (_, _) => {
		if(mediaPlayer == null || media == null) return;

		mediaPlayer.Play(media);
		mediaPlayer.Mute = false;
		playPauseButton.Content = Play(false);

		var timer = new Avalonia.Threading.DispatcherTimer {
			Interval = TimeSpan.FromMilliseconds(500)
		};

		timer.Tick += (_, _) => {
			if (mediaPlayer?.Length > 0) {
				seekBar.Value = mediaPlayer.Position * 1000;
			}
		};

		timer.Start();
	};

	private void AttachPlayPauseEvent () => playPauseButton.Click += (_, _) => {
		if (mediaPlayer == null) return;

		if (mediaPlayer.IsPlaying) {
			mediaPlayer.Pause();
			playPauseButton.Content = Play(true);
		} else {
			mediaPlayer.Play();
			playPauseButton.Content = Play(false);
		}
	};

	private void AttachVolumeEvent () => volumeBar.PropertyChanged += (_, e) => {
		if(mediaPlayer == null) return;

		if (e.Property == Slider.ValueProperty) {
			mediaPlayer.Volume = (int)volumeBar.Value;
		}
	};
	#endregion

	#region IDisposable
	private bool disposed;
	public void Dispose () {
		if (disposed) return;

		mediaPlayer?.Stop();
		videoView = null;
		media?.Dispose();
		mediaPlayer = null;
		mediaPlayer?.Dispose();

		disposed = true;

		GC.SuppressFinalize(this);
	}
	#endregion
}
