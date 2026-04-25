using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using CsharpBooru.Component;
using CsharpBooru.ViewModels;
using CsharpBooru.ViewModels.Pages;
using System;
using CsharpBooru.SQL;


namespace CsharpBooru.Views.Pages;

public partial class ViewCollectionsView : UserControl {

    private ViewCollectionsViewModel? Vm => DataContext as ViewCollectionsViewModel;
    private Collection collection;

	public ViewCollectionsView() {
        InitializeComponent();
		this.DataContextChanged += DrawCollection;

		Search_Component.Component(ref Serched).Click += (_, _) => {
			SearchSQL.SearchPosts(SearchSQL.querySearch);
			LoadPagePost();
        };

	}

    #region VIEW COLLECTION
    public void DrawCollection(object? sender, EventArgs e) {
        CollectionsListe.Children.Clear();
        
        if (Vm == null) return;
		collection = CollectionsManager.GetCollection(Vm.IdCollection);
        NameTextBox.Text = collection.Name;

		for (int i = 0; i < collection.Posts.Count; i++) {
            Button btnP = ButtonPost_Component.Component(Convert.ToInt32(collection.Posts[i]));
            int viewPostID = Convert.ToInt32(collection.Posts[i]), positionList = i;
            
            btnP.Click += (_, _) => {
				var window = this.FindAncestorOfType<Window>();
				if (window?.DataContext is MainWindowViewModel main) {
                    main.ViewImage(viewPostID, collection, positionList);
                }
            };
            CollectionsListe.Children.Add(btnP);
        }

        Button buttonAadd = new() {
            Content = new TextBlock {
                Text = "+",
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                FontSize = 24,
                FontWeight = FontWeight.ExtraBold
            },
            Width = 200,
            Height = 200,
            Margin = new Thickness(10),
            Background = Brushes.Transparent,
        };

        buttonAadd.Click += (_, _) => {
            AddCollection.IsVisible = true;
            ViewCollection.IsVisible = false;
            LoadPagePost();
        };
        CollectionsListe.Children.Add(buttonAadd);
    }

    public void ChangeName (object? sender, RoutedEventArgs e) {
        if (Vm == null) return;
        collection.Name = NameTextBox.Text ?? "Collection " + collection.Id;
		CollectionsManager.UpdateCollection(collection);
    }

    public void OnNameChange (object? sender, TextChangedEventArgs e) {
		string s = NameTextBox.Text ?? "Collection " + collection.Id;
        collection.Name = s;
	}
	#endregion

	#region ADD COLLECTION
	private int
        currentPage_AddCollection = 0,
        totalPages_AddCollection = 0;

    private void LoadPagePost () {
		SearchSQL.SearchPosts(SearchSQL.querySearch);

		PostGridList_Component pgl = new(GridPost, Click, actionButton: ActionButton);
        pgl.Descending(ref currentPage_AddCollection, ref totalPages_AddCollection);

        BuildPagination_Component.Component(PaginationPanelTop, currentPage_AddCollection, totalPages_AddCollection, page => {
            currentPage_AddCollection = page;
            LoadPagePost();
        });

        BuildPagination_Component.Component(PaginationPanelDown, currentPage_AddCollection, totalPages_AddCollection, page => {
            currentPage_AddCollection = page;
            LoadPagePost();
        });
    }

    public void Click (int index) {
        for (int cp = 0; cp < collection.Posts.Count; cp++) {
            if (collection.Posts[cp] == index) {
				collection.Posts.RemoveAt(cp);
                return;
            }
        }
		collection.Posts.Add(index);
    }

    public void ActionButton (int index, Button btn) {
        btn.Background = new SolidColorBrush(Colors.Transparent);

        for (int cp = 0; cp < collection.Posts.Count; cp++) {
            if (collection.Posts[cp] == index) {
                btn.Background = new SolidColorBrush(Colors.LightBlue);
                break;
            }
        }
    }

    public void RetunWiewCollection(object? sender, RoutedEventArgs e) {
        AddCollection.IsVisible = false;
        ViewCollection.IsVisible = true;

        if (Vm != null) CollectionsManager.UpdateCollection(collection);

        DrawCollection(sender, e);
    }
    #endregion
}