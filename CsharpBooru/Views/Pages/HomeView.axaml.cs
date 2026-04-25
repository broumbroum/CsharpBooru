using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using CsharpBooru.Component;
using CsharpBooru.SQL;
using CsharpBooru.ViewModels;
using System;

namespace CsharpBooru.Views.Pages;

public partial class HomeView : UserControl{

	public HomeView () {
		InitializeComponent();

		Search_Component.Component(ref SearchPanel).Click += (_, _) => {
			var window = this.FindAncestorOfType<Window>();
			if (window?.DataContext is MainWindowViewModel vm) vm.PostGrid();
		};

		CountPosts();
	}

	private void CountPosts () {
		foreach (char c in PostsManager.GetCount().ToString()) {
			Image img = new() {
				Width = 50,
				Height = 50
			};

			string source;

			switch (c) {
				case '0': source = "avares://CsharpBooru/Resources/Icons/number/0.png"; break;
				case '1': source = "avares://CsharpBooru/Resources/Icons/number/1.png"; break;
				case '2': source = "avares://CsharpBooru/Resources/Icons/number/2.png"; break;
				case '3': source = "avares://CsharpBooru/Resources/Icons/number/3.png"; break;
				case '4': source = "avares://CsharpBooru/Resources/Icons/number/4.png"; break;
				case '5': source = "avares://CsharpBooru/Resources/Icons/number/5.png"; break;
				case '6': source = "avares://CsharpBooru/Resources/Icons/number/6.png"; break;
				case '7': source = "avares://CsharpBooru/Resources/Icons/number/7.png"; break;
				case '8': source = "avares://CsharpBooru/Resources/Icons/number/8.png"; break;
				case '9': source = "avares://CsharpBooru/Resources/Icons/number/9.png"; break;
				default: continue;
			}

			Bitmap btm = new(AssetLoader.Open(new Uri(source)));
			img.Source = btm;
			CountPanel.Children.Add(img);
		}
	}
}