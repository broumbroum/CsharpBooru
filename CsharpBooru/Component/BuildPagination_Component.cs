using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using System;

namespace CsharpBooru.Component;

public static class BuildPagination_Component {

	private static int size = 5;

	public static void Component (StackPanel pagination, int currentPage, int totalPages, Action<int> onPageSelected) {
		if (pagination == null) return;

		pagination.Children.Clear();

		// --- Button to go to the first page ---
		Button first = PaginationButton("<<");
		first.IsEnabled = currentPage > 0;
		first.Click += (_, _) => onPageSelected(0);
		pagination.Children.Add(first);

		// --- Previous page button ---
		Button prev = PaginationButton("<");
		prev.IsEnabled = currentPage > 0;
		prev.Click += (_, _) => onPageSelected(Math.Max(0, currentPage - 1));
		pagination.Children.Add(prev);

		// --- Page window (range of visible pages) ---
		int start = Math.Max(0, currentPage - size);
		int end = Math.Min(Math.Max(0, totalPages - 1), currentPage + size);
		
		// "..."  before the page range
		if (start > 0) {
			pagination.Children.Add(new TextBlock {
				Text = "...",
				Margin = new Thickness(5),
				VerticalAlignment = VerticalAlignment.Center
			});
		}

		// Numbered page buttons
		for (int i = start; i <= end; i++) {
			if (currentPage == i) {
				TextBlock tb = new() {
					Text = (i + 1).ToString(),
					Margin = new Thickness(5),
					VerticalAlignment = VerticalAlignment.Center,
					HorizontalAlignment = HorizontalAlignment.Center,
				};
				pagination.Children.Add(tb);
			} else {
				Button bt = PaginationButton((i + 1).ToString());
				int pageIndex = i;
				bt.Click += (_, _) => onPageSelected(pageIndex);
				pagination.Children.Add(bt);
			}	
		}

		// "..." after the page range
		if (end < totalPages - 1) {
			pagination.Children.Add(new TextBlock {
				Text = "...",
				Margin = new Thickness(5),
				VerticalAlignment = VerticalAlignment.Center
			});
		}

		// --- Next page button ---
		Button next = PaginationButton(">");
		next.IsEnabled = currentPage < totalPages - 1;
		next.Click += (_, _) => onPageSelected(Math.Min(totalPages - 1, currentPage + 1));
		pagination.Children.Add(next);

		// --- Button to go to the last page ---
		Button last = PaginationButton(">>");
		last.IsEnabled = currentPage < totalPages - 1;
		last.Click += (_, _) => onPageSelected(Math.Max(0, totalPages - 1));
		pagination.Children.Add(last);
	}

	public static Button PaginationButton (string txt) => new Button {
		Content = txt,
		Margin = new Thickness(5),
		VerticalAlignment = VerticalAlignment.Center
	};
}
