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
			.Select(name => {
				int id = TagsManager.GetTagIdByName(name);
				if (id < 0) return [];

				Tag tag = TagsManager.GetTag(id);
				HashSet<int> ids = [id, .. ConvertUtils.StringToIntList(tag.Aliases ?? "")];
				return ids;
			})
			.ToList();

		var excludeIds = excludeNames
			.Select(name => {
				int id = TagsManager.GetTagIdByName(name);
				if (id < 0) return [];

				Tag tag = TagsManager.GetTag(id);
				HashSet<int> ids = [id, .. ConvertUtils.StringToIntList(tag.Aliases ?? "")];
				return ids;
			})
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

			foreach (var matchingIds in includeIds) {
				if (!matchingIds.Any(post.Tags.Contains))
					goto NextPost;
			}

			foreach (var matchingIds in excludeIds) {
				if (matchingIds.Any(post.Tags.Contains))
					goto NextPost;
			}

			if (ratingFilters.Count == 0 || ratingFilters.Any(r => post.Rating.Equals(r, StringComparison.OrdinalIgnoreCase)))
				results.Add(post.Id);
			
			NextPost:;
		}
		
		return results;
	}

}
