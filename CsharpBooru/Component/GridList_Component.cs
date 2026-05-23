using Avalonia.Controls;
using CsharpBooru.Setting;
using System;

namespace CsharpBooru.Component;
public class GridList_Component (Panel gridPost = null!) {

	private readonly Panel gridPost = gridPost;

	public event ButtonEventHandler? OnCreateButton;
	public delegate Button ButtonEventHandler (int id);

	// Display items in descending order
	public void Descending (ref int currentPage, ref int totalPages, int totalObject) {
		int pageSize = SettingValue.PostPerPage;
		gridPost.Children.Clear();

		if (totalObject == 0) return;

		totalPages = (int)Math.Ceiling(totalObject / (double)pageSize);

		if (currentPage < 0) currentPage = 0;
		if (currentPage >= totalPages) currentPage = totalPages - 1;

		int start = totalObject - ((currentPage + 1) * pageSize),
			end = totalObject - (currentPage * pageSize);

		if (start < 0) start = 0;
		if (end > totalObject) end = totalObject;

		for (int i = end - 1; i >= start; i--) {
			var b = OnCreateButton?.Invoke(i);
			if (b != null) gridPost.Children.Add(b);
		}
	}

	// Display items in ascending order
	public void Ascending (ref int currentPage, ref int totalPages, int totalObject) {
		int pageSize = SettingValue.PostPerPage;
		gridPost.Children.Clear();

		if (totalObject == 0) return;

		totalPages = (int)Math.Ceiling(totalObject / (double)pageSize);

		if (currentPage < 0) currentPage = 0;
		if (currentPage >= totalPages) currentPage = totalPages - 1;

		int start = currentPage * pageSize,
			end = start + pageSize;
		
		if(start < 0) start = 0;
		if (end > totalObject) end = totalObject;

		for (int i = start; i < end; i++) {
			var b = OnCreateButton?.Invoke(i);
			if (b != null) gridPost.Children.Add(b);
		}
	}
}
