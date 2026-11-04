using CsharpBooru.SQL;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace CsharpBooru.MCP;

[McpServerToolType]
internal sealed class CollectionsTools {
	[McpServerTool, Description("Create a new C#Booru collection, optionally initialized with existing post IDs.")]
	public static string Collection_Add (
		[Description("Name of the new collection.")] string name,
		[Description("Optional IDs of posts to include in the collection.")] int[]? posts = null) {

		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("A collection name is required.", nameof(name));

		List<int> postIds = NormalizeCollectionPosts(posts ?? []);
		var collection = new Collection(0, name.Trim(), postIds);
		CollectionsManager.AddCollection(collection);

		return "Collection '" + collection.Name + "' created with ID: " + CollectionsManager.GetCount();
	}

	[McpServerTool, Description("List all C#Booru collections with each collection ID, name, and its post IDs.")]
	public static string Collection_List () {
		var collections = CollectionsManager.GetAllCollections()
			.Select(collection => new {
				collection.Id,
				collection.Name,
				PostIds = collection.Posts,
			});

		return JsonSerializer.Serialize(collections);
	}

	[McpServerTool, Description("Retrieve a C#Booru collection by its ID, including its name and post IDs.")]
	public static string Collection_Get (
		[Description("ID of the collection to retrieve.")] int id) {

		if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Collection ID must be positive.");

		Collection collection = CollectionsManager.GetCollection(id);
		if (collection == null) throw new ArgumentException($"No collection exists with ID {id}.", nameof(id));

		return JsonSerializer.Serialize(new {
			collection.Id,
			collection.Name,
			PostIds = collection.Posts,
		});
	}

	[McpServerTool, Description("Edit the name and/or post IDs of an existing C#Booru collection. Omitted fields are left unchanged; pass an empty posts array to remove all posts.")]
	public static string Collection_Edit (
		[Description("ID of the collection to edit.")] int id,
		[Description("Replacement collection name. Omit to keep the current name.")] string? name = null,
		[Description("Replacement post IDs. Omit to keep the current posts; pass an empty array to clear them.")] int[]? posts = null) {

		if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Collection ID must be positive.");

		Collection collection = CollectionsManager.GetCollection(id);
		if (collection == null) throw new ArgumentException($"No collection exists with ID {id}.", nameof(id));

		if (name != null) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Collection name cannot be empty.", nameof(name));

			collection.Name = name.Trim();
		}

		if (posts != null) collection.Posts = NormalizeCollectionPosts(posts);

		CollectionsManager.UpdateCollection(collection);
		return $"Collection '{collection.Name}' (ID: {collection.Id}) updated.";
	}

	private static List<int> NormalizeCollectionPosts (int[] posts) {
		List<int> postIds = posts.Distinct().ToList();
		foreach (int postId in postIds) {
			if (postId <= 0)
				throw new ArgumentOutOfRangeException(nameof(posts), "Post IDs must be positive.");

			if (PostsManager.GetPost(postId) == null)
				throw new ArgumentException($"No post exists with ID {postId}.", nameof(posts));
		}

		return postIds;
	}
}
