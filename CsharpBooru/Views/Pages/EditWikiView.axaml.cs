using Avalonia.Controls;
using Avalonia.Interactivity;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using System.Linq;

namespace CsharpBooru.Views.Pages;

public partial class EditWikiView : UserControl {

	private EditWikiViewModel? Vm => DataContext as EditWikiViewModel;

	private Tag? tag;

	public EditWikiView (){
		InitializeComponent();
		Suggestions_Component.AddSuggestions(AliasesBox, MainPanel);
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
		AliasesBox.Text = string.Join(" ", (tag.Aliases ?? "")
			.Split(' ', StringSplitOptions.RemoveEmptyEntries)
			.Select(int.Parse)
			.Select(TagsManager.GetTag)
			.Where(alias => alias != null)
			.Select(alias => alias!.Name));
		ObsoleteBox.IsChecked = tag.Obsolete == "1";
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
			description: DescriptionBox.Text,
			obsolete: ObsoleteBox.IsChecked == true ? "1" : "0",
			aliases: string.Join(" ", (AliasesBox.Text ?? "")
				.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.Select(TagsManager.GetTagIdByName)
				.Where(id => id >= 0)
				.Select(TagsManager.GetTag)
				.Where(alias => alias != null)
				.Select(alias => alias!.Id.ToString()))
		);

		TagsManager.UpdateTag(_tag);

		TagsManager.CountTagsUsage();
		TagsManager.LoadTagIdCache();

		MainWindowViewModel.Main?.navigationHistory.Back();
	}
}