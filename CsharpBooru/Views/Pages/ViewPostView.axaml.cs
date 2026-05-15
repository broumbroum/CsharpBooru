using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using CsharpBooru.Component.ViewsPost;
using CsharpBooru.Setting;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsharpBooru.Views.Pages;

public partial class ViewPostView : UserControl {

	private string path = "", infoFile = "";
	private Post post;

	private const string Resources = "avares://CsharpBooru/Resources/Icons/tag/";

	private ViewPostViewModel? Vm => DataContext as ViewPostViewModel;

	public ViewPostView () {
		Core.Initialize();
		InitializeComponent();

		this.DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged (object? sender, EventArgs e) {
		if (Vm == null) return;

		post = PostsManager.GetPost(Vm.Id);
		if (post == null) return;

		path = Path.GetFullPath(DataBase.FilesPath + "/" + post.Filename).Replace("\\", "/");

		DispalyFile();
		DisplayNote();
		DisplayRelated();
		DisplayCollection();
		DisplayTag();
	}

	private void DispalyFile () {
		if (File.Exists(path) == false) {
			TextBlock textBlock = new() {
				Text = "File : " + path + " not exists !",
				Foreground = new SolidColorBrush(Colors.Red)
			};
			Post.Children.Add(textBlock);
			return;
		}

		infoFile += path + "\n";

		switch (Path.GetExtension(path).Replace("\\", "/")) {
			case ".png" or ".jpg" or ".jpeg" or ".gif" or ".ico" or ".webp" or ".tiff" or ".tif":
				Image_Component IC = new();
				Post.Children.Add(IC.Component(ref path));
				infoFile += IC.GetInfo(ref path);
			break;
			case ".mp4" or ".avi" or ".webm" or ".mkv":
				Video_Component VC = new();
				Post.Children.Add(VC.Component(ref path));
				infoFile += VC.GetInfo(ref path);
				DetachedFromVisualTree += (s, e) => VC.Dispose();
			break;
			default:
				TextBlock textBlock = new() {
					Text = "Extension not supported",
					Foreground = new SolidColorBrush(Colors.Red)
				};
				Post.Children.Add(textBlock);
			break;
		}
	}

	#region Display

	private void DisplayTag () {
		TagStack.Children.Add(CreateTextBlock("ID : " + post.Id, true, Colors.Black));

		Dictionary<string, List<string>> groups = new() {
			["Tag"] = [],
			["Artist"] = [],
			["Character"] = [],
			["Copyright"] = [],
			["Species"] = []
		};

		foreach (int tagId in post.Tags) {
			var tag = TagsManager.GetTag(tagId);
			if (tag == null) continue;

			if (!groups.ContainsKey(tag.SpecificTags))
				groups["Tag"].Add(tag.Name);
			else
				groups[tag.SpecificTags].Add(tag.Name);
		}

		AddTagGroup("Artist", Colors.OrangeRed, groups["Artist"]);
		AddTagGroup("Character", Colors.Green, groups["Character"]);
		AddTagGroup("Copyright", Colors.Magenta, groups["Copyright"]);
		AddTagGroup("Species", Colors.Red, groups["Species"]);
		AddTagGroup("Tag", Colors.Blue, groups["Tag"]);


		TagStack.Children.Add(PanelTitle(CreateTextBlock("Rating", true, Colors.Green), Resources + "icons8-law-100.png"));
		TagStack.Children.Add(CreateButtonRating(post.Rating));

		TagStack.Children.Add(PanelTitle(CreateTextBlock("Sources", true, Colors.Green), Resources + "icons8-website-100.png"));
		for (int i = 0; i < post.Sources.Count; i++) { TagStack.Children.Add(CreateButtonSources(post.Sources[i])); }

		TagStack.Children.Add(PanelTitle(CreateTextBlock("File Info", true, Colors.Green), Resources + "icons8-view-100.png"));
		TagStack.Children.Add(CreateTextBlock(infoFile, color: Colors.Black));
	}

	private void DisplayNote () {
		if (post.Note == null || post.Note == "") return;

		NoteTextBlock.Inlines =
		[
			new Run { Text = "Note\n\n", FontSize = 15, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Colors.Green) },
			new Run { Text = post.Note, Foreground = new SolidColorBrush(Colors.Black) },
		];
		NoteTextBlock.TextWrapping = TextWrapping.Wrap;
		NoteTextBlock.Margin = new(0, 50, 10, 0);
		NoteTextBlock.MaxWidth = 800;
		NoteTextBlock.Background = new SolidColorBrush(Color.Parse("#efefef"));
	}

	private void DisplayRelated () {
		if (post?.Related == null || (post.Related.Count == 0)) return;

		RelatedTextBlock.IsVisible = true;
		foreach (var _related in post.Related) {
			Post p = PostsManager.GetPost(_related);
			if (p == null) continue;
			Related.Children.Add(CreateButtonRelated(p));
		}
	}

	private void DisplayCollection () {
		if (Vm?.Collection == null || Vm.Collection.Posts.Count == 0) return;

		Collection.IsVisible = true;
		CollectionName.Text = " " + Vm.Collection.Name + "    Pages : " + (Vm.index + 1) + "/" + Vm.Collection.Posts.Count + " ";
		CollectionPrevious.Content = " < ";
		CollectionPrevious.Click += (_, _) => {
			if (Vm.index >= 1) {
				var window = this.FindAncestorOfType<Window>();
				MainWindowViewModel.main.ViewImage(Convert.ToInt32(Vm.Collection.Posts[Vm.index - 1]), Vm.Collection.Id, Vm.index - 1);
			}
		};
		CollectionNext.Content = " > ";
		CollectionNext.Click += (_, _) => {
			if (Vm.index < Vm.Collection.Posts.Count - 1) {
				var window = this.FindAncestorOfType<Window>();
				MainWindowViewModel.main.ViewImage(Convert.ToInt32(Vm.Collection.Posts[Vm.index + 1]), Vm.Collection.Id, Vm.index + 1);
			}
		};
	}

	#endregion

	#region UI

	private TextBlock CreateTextBlock (string text, bool title = false, Color color = default) {
		return new TextBlock {
			Text = text,
			FontSize = title ? 15 : 12,
			FontWeight = title ? FontWeight.Bold : FontWeight.Normal,
			Foreground = new SolidColorBrush(color),
			Margin = new Thickness(0, 10, 0, 0),
			TextWrapping = TextWrapping.Wrap
		};
	}

	private void AddTagGroup (string title, Color color, List<string> items) {
		string source = title switch {
			"Artist" => Resources + "icons8-paint-palette-100.png",
			"Character" => Resources + "icons8-customer-100.png",
			"Copyright" => Resources + "icons8-c-100.png",
			"Species" => Resources + "icons8-lapin-100.png",
			"Tag" => Resources + "icons8-tag-window-100.png",
			_ => Resources + "icons8-tag-window-100.png",
		};
		TagStack.Children.Add(PanelTitle(CreateTextBlock(title, true, color), source));

		switch (SettingValue.OrderTag) {
			case "Seizure Order": break;
			case "Alphabetical Order (ignore upper and lower case letters)":
				items.Sort(StringComparer.OrdinalIgnoreCase);
			break;
			case "Alphabetical Order":
				items.Sort(StringComparer.Ordinal);
			break;
			default:
				items.Sort(StringComparer.Ordinal);
			break;
		}

		
		foreach (var t in items)
			TagStack.Children.Add(CreateButtonTag(t, color));
	}

	private StackPanel PanelTitle (TextBlock textBlock, string image) {
		StackPanel panel = new() {
			Orientation = Orientation.Horizontal,
		};

		panel.Children.Add(new Image {
			Width = 40,
			Height = 40,
			Source = new Bitmap(AssetLoader.Open(new Uri(image))),
			Stretch = Stretch.UniformToFill

		});
		panel.Children.Add(textBlock)
			;
		return panel;
	}

	private Button CreateButtonRelated (Post post) {
		string image = ThumbnailsManager.GetThumbnailPath(post.Filename);
		Button bt = new();

		if (File.Exists(image) == true) {
			Bitmap bmp = new(image);
			Image img = new() {
				Source = bmp,
				MaxWidth = 150,
				MaxHeight = 150
			};
			bt.Content = img;
		} else {
			TextBlock tbE = new() {
				Text = "File : " + image + " not exists !",
				TextWrapping = TextWrapping.Wrap,
				MaxWidth = 150,
				MaxHeight = 150,
				Foreground = new SolidColorBrush(Colors.Red)
			};
			bt.Content = tbE;
		}
		bt.MaxWidth = 150; bt.MaxHeight = 150;
		bt.Height = 150;

		bt.Click += (s, e) => {
			MainWindowViewModel.main.ViewImage(Convert.ToInt32(post.Id.ToString()));
		};

		return bt;
	}

	private Button CreateButtonTag (string text, Color color = default) {
		int count = 0;
		int idTag = TagsManager.GetTagIdByName(text);
		if (idTag != -1) count = TagsManager.CountTagUsage(idTag);

		Button bt = new() {
			MaxHeight = 20,
			Height = 20,
			MinHeight = 20,
			Margin = new Thickness(0, 0, 0, 0),
			Background = Brushes.Transparent
		};

		TextBlock tb = new() {
			Inlines = [
				new Run { Text = text + " " },
				new Run { Text = "" + count, Foreground = new SolidColorBrush(Colors.Black) }
			],
			Foreground = new SolidColorBrush(color),
			FontSize = 12,
			ClipToBounds = false,
		};
		bt.Content = tb;

		bt.Click += (s, e) => {
			SearchSQL.querySearch = text;
			MainWindowViewModel.main.PostGrid();	
		};

		return bt;
	}

	private Button CreateButtonRating (string text) {
		Button bt = new() {
			MaxHeight = 20,
			Height = 20,
			MinHeight = 20,
			Margin = new Thickness(0, 0, 0, 0),
			Background = Brushes.Transparent
		};

		TextBlock tb = new() {
			Text = text,
			Foreground = new SolidColorBrush(Colors.Black),
			FontSize = 12,
			ClipToBounds = false,
		};
		bt.Content = tb;

		bt.Click += (s, e) => {
			SearchSQL.querySearch = "rating:" + text;
			MainWindowViewModel.main.PostGrid();
			//System.Diagnostics.Debug.WriteLine("Rating:" + text);
		};

		return bt;
	}

	private Button CreateButtonSources (string text) {
		Button bt = new() {
			MaxHeight = 20,
			Height = 20,
			MinHeight = 20,
			Margin = new Thickness(0, 0, 0, 0),
			Background = Brushes.Transparent
		};

		TextBlock tb = new() {
			Text = text,
			Foreground = new SolidColorBrush(Colors.Black),
			FontSize = 12,
			ClipToBounds = false,
		};
		bt.Content = tb;

		bt.Click += (s, e) => {
			System.Diagnostics.Debug.WriteLine("Sources:" + text);
		};

		return bt;
	}
	
	#endregion
}