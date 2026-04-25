using Avalonia.Controls;
using Avalonia.Media;
using CsharpBooru.Setting;
using CsharpBooru.ViewModels.Pages;

namespace CsharpBooru.Views.Pages;

public partial class SettingView : UserControl{

	private int postPerPage = 20;
	private bool fillThumbnailPost = false; 


	public SettingView () {

		InitializeComponent();
		DataContext = new SettingViewModel();


		PostPerPage();
		FillThumbnailPost();
	}

	private void PostPerPage () {
		postPerPage = SettingValue.PostPerPage;
		PostPerPageText.Text = "Post : " + postPerPage;

		PostPerPageScroll.Background = SolidColorBrush.Parse("#C8BFE7");
		PostPerPageScroll.Value = (double)postPerPage;
		PostPerPageScroll.ValueChanged += (source, args) => {
			postPerPage = (int)args.NewValue;
			PostPerPageText.Text = "Post : " + postPerPage;
			SettingValue.PostPerPage = postPerPage;
			SettingFile.Save();
		};
	}

	private void FillThumbnailPost () {
		fillThumbnailPost = SettingValue.FillThumbnailPost;

		FillThumbnailPostToggle.IsChecked = fillThumbnailPost;
		FillThumbnailPostToggle.Click += (source, args) => {
			fillThumbnailPost = FillThumbnailPostToggle.IsChecked ?? false;
			SettingValue.FillThumbnailPost = fillThumbnailPost;
			SettingFile.Save();
		};
	}
}