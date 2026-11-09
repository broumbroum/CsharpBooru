using CsharpBooru.SQL;
using ModelContextProtocol.Server;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace CsharpBooru.MCP;

[McpServerToolType]
internal sealed class TagsTools {
	[McpServerTool, Description("Retrieve a C#Booru tag by its ID, including its name, type, description, aliases, parent, and usage count.")]
	public static string Tags_GetById (
		[Description("ID of the tag to retrieve.")] int id) {

		if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Tag ID must be positive.");
		Tag tag = TagsManager.GetTag(id) ?? throw new ArgumentException($"No tag exists with ID {id}.", nameof(id));

		return SerializeTag(tag);
	}

	[McpServerTool, Description("Edit an existing C#Booru tag. Omitted fields are left unchanged. Alias values are tag IDs; pass an empty array to clear aliases. Use clearParent=true to remove the parent tag.")]
	public static string Tags_Edit (
		[Description("ID of the tag to edit.")] int id,
		[Description("Replacement tag name. Spaces are converted to underscores. Omit to keep the current name.")] string? name = null,
		[Description("Replacement category: Tag, Artist, Character, Copyright, or Species. Omit to keep the current category.")] string? specificTags = null,
		[Description("Replacement description. Omit to keep the current description; an empty string clears it.")] string? description = null,
		[Description("Whether the tag is obsolete. Omit to keep the current value.")] bool? obsolete = null,
		[Description("Replacement alias tag IDs. Omit to keep current aliases; pass an empty array to clear them.")] int[]? aliases = null,
		[Description("Replacement parent tag ID. Omit to keep the current parent.")] int? parentId = null,
		[Description("Set true to remove the parent tag. Cannot be combined with parentId.")] bool clearParent = false) {

		// ID
		if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Tag ID must be positive.");

		//Tag
		Tag tag = TagsManager.GetTag(id) ?? throw new ArgumentException($"No tag exists with ID {id}.", nameof(id));

		//Name
		if (name != null) {
			string normalizedName = name.Trim().Replace(' ', '_');
			if (normalizedName.Length == 0) throw new ArgumentException("Tag name cannot be empty.", nameof(name));
			tag.Name = normalizedName;
		}

		//SpecificTags
		if (specificTags != null) {
			string normalizedCategory = specificTags.Trim();
			if (normalizedCategory is not ("Tag" or "Artist" or "Character" or "Copyright" or "Species")) throw new ArgumentException("Category must be Tag, Artist, Character, Copyright, or Species.", nameof(specificTags));
			tag.SpecificTags = normalizedCategory;
		}

		//Description
		if (description != null) tag.Description = string.IsNullOrWhiteSpace(description) ? null : description;

		//Obsolete
		if (obsolete.HasValue) tag.Obsolete = obsolete.Value ? "1" : "0";

		//Parent
		if (parentId.HasValue && clearParent) throw new ArgumentException("Specify either parentId or clearParent, not both.");

		int? updatedParentId = clearParent ? null : parentId ?? tag.ParentId;
		if (updatedParentId.HasValue) {
			if (updatedParentId.Value <= 0) throw new ArgumentOutOfRangeException(nameof(parentId), "Parent tag ID must be positive.");
			if (updatedParentId.Value == id) throw new ArgumentException("A tag cannot be its own parent.", nameof(parentId));
			if (TagsManager.GetTag(updatedParentId.Value) == null) throw new ArgumentException($"No parent tag exists with ID {updatedParentId.Value}.", nameof(parentId));
		}

		//Aliases
		List<int> aliasIds = aliases == null
			? ConvertUtils.StringToIntList(tag.Aliases ?? "")
			: aliases.Distinct().ToList();

		foreach (int aliasId in aliasIds) {
			if (aliasId <= 0) throw new ArgumentOutOfRangeException(nameof(aliases), "Alias tag IDs must be positive.");
			if (aliasId == id) throw new ArgumentException("A tag cannot be its own alias.", nameof(aliases));
			if (TagsManager.GetTag(aliasId) == null) throw new ArgumentException($"No alias tag exists with ID {aliasId}.", nameof(aliases));
		}

		if (updatedParentId.HasValue && aliasIds.Contains(updatedParentId.Value)) throw new ArgumentException("A tag cannot be both a parent and an alias.");

		if (aliases != null) tag.Aliases = string.Join(" ", aliasIds);
		tag.ParentId = updatedParentId;

		TagsManager.UpdateTag(tag);
		TagsManager.LoadTagIdCache();
		return SerializeTag(TagsManager.GetTag(id));
	}

	[McpServerTool, Description("Find all tags used exactly the requested number of times across posts.")]
	public static string Tags_GetByUsageCount (
		[Description("Exact number of posts each returned tag must be used in.")] int usageCount) {

		if (usageCount < 0) throw new ArgumentOutOfRangeException(nameof(usageCount), "Usage count cannot be negative.");

		if (TagsManager.DictionaryAllTagsUsage == null) TagsManager.CountTagsUsage();
		if (TagsManager._tagIdCache == null) TagsManager.LoadTagIdCache();

		var usageCounts = TagsManager.DictionaryAllTagsUsage ?? throw new InvalidOperationException("Tag usage counts could not be loaded.");
		var tagIds = TagsManager._tagIdCache ?? throw new InvalidOperationException("The tag ID cache could not be loaded.");

		var tags = tagIds
			.Select(tag => new {
				TagName = tag.Key,
				TagId = tag.Value,
				UsageCount = usageCounts.TryGetValue(tag.Value, out int count) ? count : 0,
			})
			.Where(tag => tag.UsageCount == usageCount);

		return JsonSerializer.Serialize(tags);
	}

	private static string SerializeTag (Tag tag) => JsonSerializer.Serialize(new {
		tag.Id,
		tag.Name,
		tag.SpecificTags,
		tag.Description,
		tag.Obsolete,
		tag.Aliases,
		tag.ParentId,
		UsageCount = tag.Count,
	});
}
