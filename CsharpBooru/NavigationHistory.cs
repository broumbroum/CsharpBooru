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
			history = history.Take(currentIndex + 1).ToList();
		}

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
			MainWindowViewModel.main.HomePage();
			break;
			case "TagPage":
			MainWindowViewModel.main.TagPage();
			break;
			case "ViewImage":
			MainWindowViewModel.main.ViewImage(Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]), Convert.ToInt32(parts[3]));
			break;
			case "PostGrid":
			SQL.SearchSQL.querySearch = parts[2];
			MainWindowViewModel.main.PostGrid(Convert.ToInt32(parts[1]));
			break;
			case "CollectionsList":
			MainWindowViewModel.main.CollectionsList();
			break;
			case "EditPost":
			MainWindowViewModel.main.EditPost(Convert.ToBoolean(parts[1]), Convert.ToInt32(parts[2]));
			break;
			case "CollectionsWiew":
			MainWindowViewModel.main.CollectionsWiew(Convert.ToInt32(parts[1]));
			break;
			case "Setting":
			MainWindowViewModel.main.Setting();
			break;
			default:
			MainWindowViewModel.main.HomePage();
			break;
		}
	}
}