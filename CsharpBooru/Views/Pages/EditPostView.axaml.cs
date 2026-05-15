using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsharpBooru.Views.Pages;

public partial class EditPostView : UserControl {

	private string path = "",tag = "", rating = "none", note = "";
	private List<string> sources = [];
	private List<int> relatedsID = [];

	private EditPostViewModel? Vm => DataContext as EditPostViewModel;

	public EditPostView () { 
		InitializeComponent();

		this.DataContextChanged += InitializeVariable;

		RatingBox.SelectionChanged += (_, _) => {
			var valeur = (RatingBox.SelectedItem as ComboBoxItem)?.Content;
			rating = valeur?.ToString()?.ToLower() ?? "none";
			System.Diagnostics.Debug.WriteLine($"Sélection : {valeur}");
		};

		Search_Component.Component(ref Serched).Click += (_,_) => {
			LoadPagePost();
			SearchSQL.SearchPosts(SearchSQL.querySearch);
		};

	}

	private void InitializeVariable (object? sender, EventArgs e) {
		if (Vm == null) {
			return;
		} else if (Vm.EditMod == true) {
			Post post = PostsManager.GetPost(Vm.IdItem);
			if (post == null) return;

			path = DataBase.FilesPath + post.Filename;
			PathText.Text = path;

			tag = " ";
			for (int i = 0; i < post.Tags.Count; i++) {
				Tag t = TagsManager.GetTag(post.Tags[i]);
				tag +=  t.Name + " ";
			}
			TagBox.Text = tag;

			rating = post.Rating;
			RatingBox.SelectedIndex = rating switch {
				"none" => 0,
				"safe" => 1,
				"questionable" => 2,
				"explicit" => 3,
				"borderline" => 4,
				_ => 0,
			};

			note = post.Note;
			NoteText.Text = note;

			sources = post.Sources;
			string _source = "";
			for (int i = 0; i < sources.Count; i++) {
				_source = _source + sources[i] + "\n";
			}
			SourceBox.Text = _source;

			relatedsID = post.Related;
			RelatedTextUpdate();
		}
	}

	private async void SelectFile (object? sender, RoutedEventArgs e) {

		var topLevel = TopLevel.GetTopLevel(this);
		if (topLevel == null) return;

		var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions {
			AllowMultiple = false,
			Title = "Select a File"
		});

		if (files.Count == 0)
			return;

		string path = files[0].Path.LocalPath;

		this.path = path;
		PathText.Text = path;

	}

	private void OnTagChanged (object? sender, TextChangedEventArgs e) {
		if (sender is TextBox tb) tag = tb.Text ?? "";
	}

	private void OnSourceChanged (object? sender, TextChangedEventArgs e) {
		string s = SourceBox.Text ?? "";
		sources = s.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
	}

	private void OnNoteChanged (object? sender, TextChangedEventArgs e) {
		if (sender is TextBox tb) note = tb.Text ?? "";
	}

	private void GridRelate (object? sender, RoutedEventArgs e) {
		RelatedPost.IsVisible = true;
		Main.IsVisible = false;
		LoadPagePost();
	}

	private void SavePost (object? sender, RoutedEventArgs e) {
		if (Vm == null) return;

		if (path == null || path == "") {
			Error.Text = "No file selected";
			return; 
		}
		if (tag == null || tag == "") {
			Error.Text = "No tag";
			return;
		}

		string file = Path.GetFileName(path);

		List<int> TagID = new();
		foreach (string t in tag.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
			TagID.Add(TagsManager.GetOrCreateTag(t));
		}

		if (Vm.EditMod == false) {
			Post _post = new (0, FileManager.SaveFile(this.path), TagID, sources, relatedsID, note, rating);
			PostsManager.AddPost(_post);
		} else {
			Post _post = new(Vm.IdItem, FileManager.SaveFile(this.path), TagID, sources, relatedsID, note, rating);
			PostsManager.UpdatePost(_post);
		}

		MainWindowViewModel.main.HomePage();
	}

	#region GridRelated
	private int
		_currentPage = 0,
		_totalPages = 0;

	private void LoadPagePost () {
		SearchSQL.SearchPosts(SearchSQL.querySearch);

		PostGridList_Component pgl = new(GridPost, Click, actionButton: ActionButton);
		pgl.Descending(ref _currentPage, ref _totalPages);

		BuildPagination_Component.Component(PaginationPanelTop, _currentPage, _totalPages, page => {
			_currentPage = page;
			LoadPagePost();
		});

		BuildPagination_Component.Component(PaginationPanelDown, _currentPage, _totalPages, page => {
			_currentPage = page;
			LoadPagePost();
		});
	}

	private void Click (int index) {
		if (Vm?.IdItem == index) return;

		for (int r = 0; r < relatedsID.Count; r++) {
			if (relatedsID[r] == index) {
				relatedsID.RemoveAt(r);
				goto Exite;
			}
		}
		relatedsID.Add(index);
	Exite:;
		RelatedTextUpdate();

	}

	private void ActionButton (int index, Button btn) {
		btn.Background = new SolidColorBrush(Colors.Transparent);

		if (Vm?.IdItem == index) {
			btn.Background = new SolidColorBrush(Colors.Red);
			return;
		}

		for (int a = 0; a < relatedsID.Count; a++) {
			//System.Diagnostics.Debug.WriteLine("relateds[a] : " + relateds[a] + " Index : " + index);
			if (relatedsID[a] == index) {
				btn.Background = new SolidColorBrush(Colors.LightBlue);
				break;
			}
		}
	}

	private void RetunMain (object? sender, RoutedEventArgs e) {
		RelatedPost.IsVisible = false;
		Main.IsVisible = true;
	}

	private void RelatedTextUpdate () {
		string str = "";
		for (int i = 0; i < relatedsID.Count; i++) {
			str += "ID:" + relatedsID[i] + " ";
		}
		RelatedText.Text = str;
	}
	#endregion
}