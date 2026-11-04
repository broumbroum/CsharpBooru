using CsharpBooru.SQL;
using ModelContextProtocol.Server;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace CsharpBooru.MCP;

[McpServerToolType]
internal sealed class TagsTools {
	[McpServerTool, Description("Find all tags used exactly the requested number of times across posts.")]
	public static string Tags_GetByUsageCount (
		[Description("Exact number of posts each returned tag must be used in.")] int usageCount) {

		if (usageCount < 0)
			throw new ArgumentOutOfRangeException(nameof(usageCount), "Usage count cannot be negative.");

		if (TagsManager.DictionaryAllTagsUsage == null)
			TagsManager.CountTagsUsage();
		if (TagsManager._tagIdCache == null)
			TagsManager.LoadTagIdCache();

		var usageCounts = TagsManager.DictionaryAllTagsUsage
			?? throw new InvalidOperationException("Tag usage counts could not be loaded.");
		var tagIds = TagsManager._tagIdCache
			?? throw new InvalidOperationException("The tag ID cache could not be loaded.");

		var tags = tagIds
			.Select(tag => new {
				TagName = tag.Key,
				TagId = tag.Value,
				UsageCount = usageCounts.TryGetValue(tag.Value, out int count) ? count : 0,
			})
			.Where(tag => tag.UsageCount == usageCount);

		return JsonSerializer.Serialize(tags);
	}
}
