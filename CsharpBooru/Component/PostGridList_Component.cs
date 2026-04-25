using Avalonia.Controls;
using CsharpBooru.Setting;
using System;
using System.Collections.Generic;
using CsharpBooru.SQL;

namespace CsharpBooru.Component;
public class PostGridList_Component {

	private int pageSize = 20;
	private List<int> _filteredIndexes = [];

	private readonly WrapPanel gridPost;
	private readonly Action<int> Click;
	private readonly Func<int, ContextMenu?>? contextMenu;
	private readonly Action<int, Button>? actionButton;

	public PostGridList_Component (WrapPanel gridPost, Action<int> Click, Func<int, ContextMenu?>? contextMenu = null, Action<int, Button>? actionButton = null) {
		this.gridPost = gridPost;
		this.Click = Click;
		this.contextMenu = contextMenu;
		this.actionButton = actionButton;
	}

	public void Descending (ref int currentPage, ref int totalPages) {
		gridPost.Children.Clear();
		pageSize = SettingValue.PostPerPage;

		_filteredIndexes = SearchSQL.SearchPosts(SearchSQL.querySearch);
		if (_filteredIndexes.Count == 0) return;

		int totalFiltered = _filteredIndexes.Count;
		totalPages = (int)Math.Ceiling(totalFiltered / (double)pageSize);

		int start = totalFiltered - ((currentPage + 1) * pageSize),
			end = totalFiltered - (currentPage * pageSize);
		if (start < 0) start = 0;

		for (int i = end - 1; i >= start; i--) {
			int postIndex = _filteredIndexes[i];

			Button btnP = ButtonPost_Component.Component(postIndex);
			btnP.Click += (_, _) => Click(postIndex);

			// Add context menu if provided
			if (contextMenu != null) btnP.ContextMenu = contextMenu(postIndex);

			// Add an action to the button
			if (actionButton != null) {
				btnP.Click += (_, _) => actionButton(postIndex, btnP);
				actionButton(postIndex, btnP);
			}

			gridPost.Children.Add(btnP);
		}
	}
}
