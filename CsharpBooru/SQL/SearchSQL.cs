using System;
using System.Collections.Generic;
using System.Linq;

namespace CsharpBooru.SQL;
internal class SearchSQL {

	public static string querySearch = "";

	public static (List<string> include, List<string> exclude) ParseSearch (string input) {
		var include = new List<string>();
		var exclude = new List<string>();

		var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		foreach (var p in parts) {
			if (p.StartsWith("-"))
				exclude.Add(p.Substring(1));
			else
				include.Add(p);
		}

		return (include, exclude);
	}

	public static List<int> SearchPosts (string query) {
		var (includeNames, excludeNames) = ParseSearch(query);

		// Convertir noms → IDs
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

			bool includeOK = includeIds.All(id => postTags.Contains(id));
			bool excludeOK = excludeIds.All(id => !postTags.Contains(id));

			if (includeOK && excludeOK)
				results.Add(post.Id);
		}

		return results;
	}

}
