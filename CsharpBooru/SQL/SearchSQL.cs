using System;
using System.Collections.Generic;
using System.Linq;

namespace CsharpBooru.SQL;
internal class SearchSQL {

	public static string querySearch = "";

	public static (List<string> include, List<string> exclude, List<string> ratings) ParseSearch (string input) {
		var include = new List<string>();
		var exclude = new List<string>();
		var ratings = new List<string>();

		var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		foreach (var p in parts) {
			if (p.StartsWith("rating:", StringComparison.OrdinalIgnoreCase)) {
				var r = p.Substring("rating:".Length).Trim();
				if (!string.IsNullOrWhiteSpace(r))
					ratings.Add(r);
			} else if (p.StartsWith("-"))
				exclude.Add(p.Substring(1));
			else
				include.Add(p);
		}

		return (include, exclude, ratings);
	}

	public static List<int> SearchPosts (string query) {
		if (query == null || query == "") return [.. PostsManager.GetAllPosts().Select(p => p.Id)];
		var (includeNames, excludeNames, ratingFilters) = ParseSearch(query);


		var includeIds = includeNames
			.Select(n => TagsManager.GetTagIdByName(n))
			.Where(id => id != null)
			.Select(id => id.Value)
			.ToList();

		var excludeIds = excludeNames
			.Select(n => TagsManager.GetTagIdByName(n))
			.Where(id => id != null)
			.Select(id => id.Value)
			.ToList();

		var results = new List<int>();

		foreach (var post in PostsManager.GetAllPosts()) {
			var postTags = post.Tags; 

			bool includeOK = !includeIds.All(id => !postTags.Contains(id));
			bool excludeOK = excludeIds.All(id => !postTags.Contains(id));
			bool ratingOK = ratingFilters.Count == 0 || ratingFilters.Any(r => post.Rating.Equals(r, StringComparison.OrdinalIgnoreCase));

			if (includeOK && excludeOK && ratingOK)
				results.Add(post.Id);
		}

		return results;
	}

}
