using CsharpBooru.SQL;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

namespace CsharpBooru.MCP;

[McpServerToolType]
internal sealed class SearchTool {
	[McpServerTool, Description("Search C#Booru posts by tags and filters. Supports tag names, -tag exclusions, rating:value, extension:value, and note:true or note:false. Returns matching post IDs.")]
	public static string Search_Posts (
		[Description("Search query, for example: cat rating:safe extension:jpg -dog")] string query = "") {

		var postIds = SearchSQL.SearchPosts(query);
		return JsonSerializer.Serialize(postIds);
	}

	[McpServerTool, Description("Search C#Booru posts by tags and filters. Returns up to the requested number of newest matching posts with their details.")]
	public static string Search_PostsWithDetails (
		[Description("Search query, for example: cat rating:safe extension:jpg -dog")] string query = "",
		[Description("Maximum number of newest matching posts to return.")] int limit = 20) {

		if (limit <= 0) limit = Int32.MaxValue;

		var postIds = SearchSQL.SearchPosts(query).OrderByDescending(id => id).Take(limit);

		var posts = new List<Post>();
		foreach (int id in postIds) {
			Post post = PostsManager.GetPost(id) ?? throw new InvalidOperationException($"Post with ID {id} was not found.");
			posts.Add(post);
		}

		return JsonSerializer.Serialize(posts);
	}
}