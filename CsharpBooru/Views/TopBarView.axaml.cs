using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using CsharpBooru.ViewModels;

namespace CsharpBooru.Views;

public partial class TopBarView : UserControl {

	public TopBarView () {
		InitializeComponent();
		TopBar.Background = SolidColorBrush.Parse("#C8BFE7");
	}

	private void HomeButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.main.HomePage();
	private void TagButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.main.TagPage();
	private void PostsButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.main.PostGrid();
	private void AddPostButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.main.EditPost();
	private void CollectionsButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.main.CollectionsList();
	private void SettingButtonClick (object sender, RoutedEventArgs args) => MainWindowViewModel.main.Setting();
}