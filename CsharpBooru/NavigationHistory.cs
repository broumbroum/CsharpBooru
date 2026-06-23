using CsharpBooru.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CsharpBooru;
public class NavigationHistory {
	private List<string> history = ["HomePage"];
	private int currentIndex = 0;
	private bool isNavigating = false;

	public void AddPage (string page) {
		if (isNavigating) return;
		if (currentIndex < history.Count - 1) {
			history = [.. history.Take(currentIndex + 1)];
		}

		if (history[^1] == page) return;

		history.Add(page);
		currentIndex++;
	}

	public void Back () {
		if (currentIndex > 0) {
			isNavigating = true;
			currentIndex--;
			ToPage();
			isNavigating = false;
		}
	}

	public void Forward () {
		if (currentIndex < history.Count - 1) {
			isNavigating = true;
			currentIndex++;
			ToPage();
			isNavigating = false;
		}
	}

	private void ToPage () {
		string[] parts = history[currentIndex].Split('&');

		switch (parts[0]) {
			case "HomePage":
			MainWindowViewModel.Main?.HomePage();
			break;
			case "TagPage":
			MainWindowViewModel.Main?.TagPage();
			break;
			case "ViewImage":
			MainWindowViewModel.Main?.ViewImage(Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]), Convert.ToInt32(parts[3]));
			break;
			case "PostGrid":
			SQL.SearchSQL.querySearch = parts[2];
			MainWindowViewModel.Main?.PostGrid(Convert.ToInt32(parts[1]));
			break;
			case "CollectionsList":
			MainWindowViewModel.Main?.CollectionsList(Convert.ToInt32(parts[1]));
			break;
			case "EditPost":
			MainWindowViewModel.Main?.EditPost(Convert.ToBoolean(parts[1]), Convert.ToInt32(parts[2]));
			break;
			case "CollectionsWiew":
			MainWindowViewModel.Main?.CollectionsWiew(Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]));
			break;
			case "Setting":
			MainWindowViewModel.Main?.Setting();
			break;
			case "About":
			MainWindowViewModel.Main?.About();
			break;
			case "Wiki":
			MainWindowViewModel.Main?.Wiki(Convert.ToInt32(parts[1]));
			break;
			case "EditWiki":
			MainWindowViewModel.Main?.EditWiki(Convert.ToInt32(parts[1]));
			break;
			default:
			MainWindowViewModel.Main?.HomePage();
			break;
		}
	}
}