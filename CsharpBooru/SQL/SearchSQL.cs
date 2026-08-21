using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsharpBooru.SQL;
internal class SearchSQL {

	public static string querySearch = "";

	public static (List<string> include, List<string> exclude, List<string> ratings, List<string> extension) ParseSearch (string input) {
		List<string>?
			include = [],
			exclude = [],
			ratings = [],
			extension = [];

		var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		foreach (var p in parts) {
			if (p.StartsWith("rating:", StringComparison.OrdinalIgnoreCase)) {
				var r = p["rating:".Length..].Trim();
				if (!string.IsNullOrWhiteSpace(r))
					ratings.Add(r);
			}else if (p.StartsWith("extension:", StringComparison.OrdinalIgnoreCase)) {
				var r = p["extension:".Length..].Trim();
				if (!string.IsNullOrEmpty(r))
					extension.Add(r);
			}else if (p.StartsWith('-'))
				exclude.Add(p[1..]);
			else
				include.Add(p);
		}

		return (include, exclude, ratings, extension);
	}

	public static List<int> SearchPosts (string query) {
		if (query == null || query == "") return [.. PostsManager.GetAllPosts().Select(p => p.Id)];
		var (includeNames, excludeNames, ratingFilters, extensionFilters) = ParseSearch(query);


		var includeIds = includeNames
			.Select(n => TagsManager.GetTagIdByName(n))
			.ToList();

		var excludeIds = excludeNames
			.Select(n => TagsManager.GetTagIdByName(n))
			.ToList();

		var results = new List<int>();

		Post post;
		foreach (var idP in PostsManager.GetAllPostIds()) {
			post = PostsManager.GetPost(idP);

			foreach (var extensionFilter in extensionFilters) {
				if (Path.GetExtension(post.Filename) == ("." + extensionFilter))
					continue;
				else
					goto NextPost;
			}

			foreach (var includeId in includeIds) {
				if (includeId == -1)
					goto NextPost;
				else if (post.Tags.Contains(includeId))
					continue;
				else
					goto NextPost;
			}

			foreach (var excludeId in excludeIds) {
				if(excludeId == -1)
					goto NextPost;
				else if (!post.Tags.Contains(excludeId))
					continue;
				else
					goto NextPost;
			}

			if (ratingFilters.Count == 0 || ratingFilters.Any(r => post.Rating.Equals(r, StringComparison.OrdinalIgnoreCase)))
				results.Add(post.Id);
			
			NextPost:;
		}
		
		return results;
	}

}
