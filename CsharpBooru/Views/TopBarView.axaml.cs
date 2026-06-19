using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CsharpBooru.ViewModels;
using System;

namespace CsharpBooru.Views;

public partial class TopBarView : UserControl {

	public TopBarView () {
		InitializeComponent();
		TopBar.Background = SolidColorBrush.Parse("#C8BFE7");

		BackButton.Content = new Image { 
			Source = new Bitmap(AssetLoader.Open(new Uri("avares://CsharpBooru/Resources/Icons/arrow/icons8-left-arrow-64.png"))),
			Width = 30, Height = 30 
		};
		ForwardButton.Content = new Image {
			Source = new Bitmap(AssetLoader.Open(new Uri("avares://CsharpBooru/Resources/Icons/arrow/icons8-flèche-64.png"))),
			Width = 30, Height = 30 
		};
	}

	private void BackButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.navigationHistory.Back();
	private void ForwardButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.navigationHistory.Forward();

	private void HomeButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.HomePage();
	private void TagButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.TagPage();
	private void PostsButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.PostGrid();
	private void AddPostButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.EditPost();
	private void CollectionsButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.CollectionsList();
	private void SettingButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.Setting();
	private void AboutButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.Main?.About();
}