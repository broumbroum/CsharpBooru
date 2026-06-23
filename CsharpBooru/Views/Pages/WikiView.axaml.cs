using Avalonia.Controls;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels.Pages;
using CsharpBooru.Component;
using System;
using System.Collections.Generic;
using CsharpBooru.ViewModels;

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

		NameTextBlock.Text = tag.Name;
		SpecificTagsTextBlock.Text = "Category : " + tag.SpecificTags ?? "Tag";
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