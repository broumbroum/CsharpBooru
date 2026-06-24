using Avalonia.Controls;
using Avalonia.Media;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using System.Collections.Generic;

namespace CsharpBooru.Views.Pages;

public partial class WikiView : UserControl {

	private WikiViewModel? Vm => DataContext as WikiViewModel;

	private Tag? tag;
	public WikiView() {
        InitializeComponent();
		DataContextChanged += OnDataContextChanged;

	}

	private void OnDataContextChanged (object? sender, EventArgs e) {
		if (Vm == null) return;
		tag = TagsManager.GetTag(Vm.CurrentID);

		NameTextBlock.Text = tag.Name.Replace('_',' ');
		SpecificTagsTextBlock.Text = "Category : " + tag.SpecificTags ?? "Tag";
		SpecificTagsTextBlock.Foreground = tag.SpecificTags switch {
			"Tag" => Brushes.Blue,
			"Artist" => Brushes.OrangeRed,
			"Character" => Brushes.Green,
			"Copyright" => Brushes.Magenta,
			"Species" => Brushes.Red,
			_ => Brushes.Black,
		};
		DescriptionTextBlock.Text = tag.Description ?? "";
		GetExample();
	}

	private void GetExample () {
		List<Post> posts = PostsManager.GetLastPostsByTag(tag!.Id);

		for (int i = 0; i < posts.Count; i++) {
			int postId = posts[i].Id;

			Button btn = ButtonPost_Component.Component(postId);
			btn.Click += (_, _) => MainWindowViewModel.Main?.ViewImage(postId, null, -1);

			ExamplePanel.Children.Add(btn);
		}
		
	}
}