using CsharpBooru.SQL;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CsharpBooru.MCP;

[McpServerToolType]
internal sealed class PostsTools {
	[McpServerTool, Description("Add a local file as a new C#Booru post. The file is copied into the configured library.")]
	public static string Post_Add (
		[Description("Absolute path to the image, animation, or video file accessible to the C#Booru process.")] string filePath,
		[Description("One or more tag names. Spaces in tag names are converted to underscores. At least one tag is required.")] string[] tags,
		[Description("Optional post rating: none, safe, questionable, explicit, or borderline.")] string rating = "none",
		[Description("Optional note for the post.")] string note = "",
		[Description("Optional source URLs or text.")] string[]? sources = null,
		[Description("Optional IDs of existing related posts.")] int[]? related = null) {

		//File path
		if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("A file path is required.", nameof(filePath));

		string fullPath = Path.GetFullPath(filePath);
		if (!File.Exists(fullPath)) throw new FileNotFoundException("The source file was not found.", fullPath);
		
		//Tags
		if (tags == null) throw new ArgumentNullException(nameof(tags));
		string[] normalizedTags = NormalizeTags(tags);

		//Rating
		if (string.IsNullOrWhiteSpace(rating)) rating = "none";
		string normalizedRating = rating.Trim().ToLowerInvariant();
		if (normalizedRating is not ("none" or "safe" or "questionable" or "explicit" or "borderline"))
			throw new ArgumentException("Rating must be none, safe, questionable, explicit, or borderline.", nameof(rating));
		List<int> relatedIds = NormalizeRelatedPosts(related ?? []);

		string filename = FileManager.SaveFile(fullPath);
		bool postAdded = false;
		try {
			var tagIds = normalizedTags.Select(TagsManager.GetOrCreateTag).ToList();
			string[] normalizedSources = (sources ?? [])
				.Select(source => source?.Trim() ?? "")
				.Where(source => source.Length > 0)
				.ToArray();

			PostsManager.AddPost(new Post(0, filename, tagIds, [.. normalizedSources], relatedIds, note ?? "", normalizedRating));
			postAdded = true;

			TagsManager.CountTagsUsage();
			TagsManager.LoadTagIdCache();

			return "Post added successfully: " + filename + " with ID: " + PostsManager.GetCount();
		}
		finally {
			if (!postAdded)
				FileManager.DeleteFile(filename);
		}
	}

	[McpServerTool, Description("Retrieve a C#Booru post by its ID, including its filename, tags, sources, rating, note, and related post IDs.")]
	public static string Post_Get (
		[Description("ID of the post to retrieve.")] int id) {

		if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Post ID must be positive.");
		Post post = PostsManager.GetPost(id) ?? throw new ArgumentException($"No post exists with ID {id}.", nameof(id));
		
		return SerializePost(post);
	}
	
	[McpServerTool, Description("Edit the tags or metadata of an existing C#Booru post. Omitted fields are left unchanged; the file is preserved.")]
	public static string Post_Edit (
		[Description("ID of the post to edit.")] int id,
		[Description("Replacement tag names. Spaces are converted to underscores. Omit to keep the current tags.")] string[]? tags = null,
		[Description("Replacement rating: none, safe, questionable, explicit, or borderline. Omit to keep the current rating.")] string? rating = null,
		[Description("Replacement note. Omit to keep the current note.")] string? note = null,
		[Description("Replacement source URLs or text. Omit to keep the current sources.")] string[]? sources = null,
		[Description("Replacement IDs of related posts. Omit to keep the current related posts; pass an empty array to clear them.")] int[]? related = null) {

		//ID
		if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Post ID must be positive.");

		//Post
		Post post = PostsManager.GetPost(id) ?? throw new ArgumentException($"No post exists with ID {id}.", nameof(id));

		//Tags
		if (tags != null) {
			string[] normalizedTags = NormalizeTags(tags);
			post.Tags = normalizedTags.Select(TagsManager.GetOrCreateTag).ToList();
		}

		//Rating
		if (rating != null) {
			if (string.IsNullOrWhiteSpace(rating)) throw new ArgumentException("Rating cannot be empty.", nameof(rating));

			string normalizedRating = rating.Trim().ToLowerInvariant();
			if (normalizedRating is not ("none" or "safe" or "questionable" or "explicit" or "borderline"))
				throw new ArgumentException("Rating must be none, safe, questionable, explicit, or borderline.", nameof(rating));

			post.Rating = normalizedRating;
		}
		
		//Note
		if (note != null) post.Note = note;

		//Sources
		if (sources != null) {
			post.Sources = sources
				.Select(source => source?.Trim() ?? "")
				.Where(source => source.Length > 0)
				.ToList();
		}

		//Related
		List<int>? normalizedRelated = related == null ? null : NormalizeRelatedPosts(related, id);
		if (normalizedRelated != null) post.Related = normalizedRelated;

		PostsManager.UpdatePost(post);
		TagsManager.CountTagsUsage();
		TagsManager.LoadTagIdCache();

		return SerializePost(post);
	}

	private static string[] NormalizeTags (string[] tags) {
		string[] normalizedTags = tags
			.Select(tag => (tag?.Trim() ?? "").Replace(' ', '_'))
			.Where(tag => tag.Length > 0)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToArray();

		if (normalizedTags.Length == 0)
			throw new ArgumentException("At least one non-empty tag is required.", nameof(tags));

		return normalizedTags;
	}

	private static List<int> NormalizeRelatedPosts (int[] related, int? currentPostId = null) {
		List<int> relatedIds = related.Distinct().ToList();
		foreach (int relatedId in relatedIds) {
			if (relatedId <= 0)
				throw new ArgumentOutOfRangeException(nameof(related), "Related post IDs must be positive.");

			if (relatedId == currentPostId)
				throw new ArgumentException("A post cannot be related to itself.", nameof(related));

			if (PostsManager.GetPost(relatedId) == null)
				throw new ArgumentException($"No related post exists with ID {relatedId}.", nameof(related));
		}

		return relatedIds;
	}

	private static string SerializePost (Post post) {
		string[] tagNames = post.Tags
			.Select(tagId => TagsManager.GetTag(tagId))
			.Select(tag => tag?.Name ?? throw new InvalidOperationException("The post references a tag that does not exist."))
			.ToArray();

		return JsonSerializer.Serialize(new {
			post.Id,
			post.Filename,
			Tags = tagNames,
			post.Sources,
			Related = post.Related,
			post.Rating,
			post.Note,
		});
	}
}
