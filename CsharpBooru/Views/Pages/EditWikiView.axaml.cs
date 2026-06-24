using Avalonia.Controls;
using Avalonia.Interactivity;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;

namespace CsharpBooru.Views.Pages;

public partial class EditWikiView : UserControl {

	private EditWikiViewModel? Vm => DataContext as EditWikiViewModel;

	private Tag? tag;

	public EditWikiView (){
		InitializeComponent();
		DataContextChanged += OnDataContextChanged;
	}

	private void OnDataContextChanged (object? sender, EventArgs e) {
		if (Vm == null) return;
		tag = TagsManager.GetTag(Vm.IdTag);

		if(tag == null) return;

		NameTagBox.Text = tag.Name;

		CategoryBox.SelectedIndex = tag.SpecificTags switch {
			"Tag" => 0,
			"Artist" => 1,
			"Character" => 2,
			"Copyright" => 3,
			"Species" => 4,
			_ => 0,
		};

		DescriptionBox.Text = tag.Description ?? "";
	}

	public void OnSavePostClick (object? sender, RoutedEventArgs e) {
		if (tag == null) return;

		if (NameTagBox.Text == null || NameTagBox.Text == "") {
			Error.Text = "No name tag";
			return; 
		}

		Tag _tag = new(
			id: tag.Id,
			specificTags: CategoryBox.SelectedIndex switch {
				0 => "Tag",
				1 => "Artist",
				2 => "Character",
				3 => "Copyright",
				4 => "Species",
				_ => "Tag",
			},
			name: NameTagBox.Text.Replace(" ", "_"),
			description: DescriptionBox.Text
		);

		TagsManager.UpdateTag(_tag);
		MainWindowViewModel.Main?.navigationHistory.Back();
	}
}