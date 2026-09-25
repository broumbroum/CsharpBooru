using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace CsharpBooru.SQL;
public static class TagsManager {

	// ➕ Add a tag
	public static void AddTag (string name, string specificTags) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		int nextId;

		using (var cmd1 = new SQLiteCommand("SELECT id FROM Tags ORDER BY id ASC", conn))
		using (var reader = cmd1.ExecuteReader()) {
			var used = new HashSet<int>();

			while (reader.Read())
				used.Add(reader.GetInt32(0));

			//Find the first missing ID (1, 2, 3, ...)
			nextId = Enumerable.Range(1, used.Count + 1)
							   .First(i => !used.Contains(i));
		}

		string sql = @"
			INSERT INTO Tags (id, name, specificTags, description, obsolete, aliases)
			VALUES (@id, @name, @specificTags, @description, @obsolete, @aliases);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("id", nextId);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@specificTags", specificTags);
		cmd.Parameters.AddWithValue("@description", DBNull.Value);
		cmd.Parameters.AddWithValue("@obsolete", "0");
		cmd.Parameters.AddWithValue("@aliases", DBNull.Value);

		cmd.ExecuteNonQuery();
	}

	// ➕ Add a tag or retrieve its ID if it already exists
	public static int GetOrCreateTag (string name) {
		using SQLiteConnection? conn = DataBase.GetConnection();
		conn.Open();

		// 1. Check if the tag already exists.
		string checkSql = "SELECT id FROM Tags WHERE name = @name";

		using (var checkCmd = new SQLiteCommand(checkSql, conn)) {
			checkCmd.Parameters.AddWithValue("@name", name);

			var result = checkCmd.ExecuteScalar();
			if (result != null) {
				return Convert.ToInt32(result);
			}
		}

		// 2. Find the first free ID
		int nextId;

		using (var cmd1 = new SQLiteCommand("SELECT id FROM Tags ORDER BY id ASC", conn))
		using (var reader = cmd1.ExecuteReader()) {
			var used = new HashSet<int>();

			while (reader.Read())
				used.Add(reader.GetInt32(0));

			nextId = Enumerable.Range(1, used.Count + 1)
							   .First(i => !used.Contains(i));
		}

		// 3. The tag does not exist → create it with specificTags = "Tag"
		string sql = @"
			INSERT INTO Tags (id, name, specificTags, description, obsolete, aliases)
			VALUES (@id, @name, @specificTags, @description, @obsolete, @aliases);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", nextId);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@specificTags", "Tag");
		cmd.Parameters.AddWithValue("@description", DBNull.Value);
		cmd.Parameters.AddWithValue("@obsolete", "0");
		cmd.Parameters.AddWithValue("@aliases", DBNull.Value);

		cmd.ExecuteNonQuery();

		// 4. Retrieve the ID of the new tag
		return nextId;
	}

	// ✏️ Edit a tag
	public static void UpdateTag (Tag tag) {
		using var conn = DataBase.GetConnection();
		conn.Open();
		using var transaction = conn.BeginTransaction();

		string sql = @"
			UPDATE Tags
			SET name = @name,
				specificTags = @specificTags,
				description = @description,
				obsolete = @obsolete,
				aliases = @aliases
			WHERE id = @id;
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Transaction = transaction;
		cmd.Parameters.AddWithValue("@id", tag.Id);
		cmd.Parameters.AddWithValue("@name", tag.Name);
		cmd.Parameters.AddWithValue("@specificTags", tag.SpecificTags);
		cmd.Parameters.AddWithValue("@description", (object?)tag.Description ?? DBNull.Value);
		cmd.Parameters.AddWithValue("@obsolete", tag.Obsolete);
		cmd.Parameters.AddWithValue("@aliases", SerializeAliasIds(
			ParseAliasIds(tag.Aliases).Where(id => id != tag.Id)));

		cmd.ExecuteNonQuery();

		HashSet<int> aliases = ParseAliasIds(tag.Aliases);
		aliases.Remove(tag.Id);

		using var readTags = new SQLiteCommand("SELECT id, aliases FROM Tags WHERE id <> @id", conn, transaction);
		readTags.Parameters.AddWithValue("@id", tag.Id);
		using var reader = readTags.ExecuteReader();
		var aliasMap = new Dictionary<int, HashSet<int>> {
			[tag.Id] = aliases,
		};

		while (reader.Read()) {
			int otherId = Convert.ToInt32(reader["id"]);
			HashSet<int> otherAliases = ParseAliasIds(
				reader["aliases"] == DBNull.Value ? null : reader["aliases"].ToString());
			bool shouldContainCurrentTag = aliases.Contains(otherId);

			if (shouldContainCurrentTag)
				otherAliases.Add(tag.Id);
			else
				otherAliases.Remove(tag.Id);

			aliasMap[otherId] = otherAliases;
		}

		reader.Close();

		var connections = aliasMap.Keys.ToDictionary(id => id, _ => new HashSet<int>());
		foreach ((int id, HashSet<int> relatedIds) in aliasMap) {
			foreach (int relatedId in relatedIds) {
				if (!connections.ContainsKey(relatedId)) continue;
				connections[id].Add(relatedId);
				connections[relatedId].Add(id);
			}
		}

		var aliasGroup = new HashSet<int> { tag.Id };
		var pending = new Queue<int>();
		pending.Enqueue(tag.Id);
		while (pending.Count > 0) {
			int id = pending.Dequeue();
			foreach (int relatedId in connections[id]) {
				if (aliasGroup.Add(relatedId))
					pending.Enqueue(relatedId);
			}
		}

		using var updateReciprocal = new SQLiteCommand(
			"UPDATE Tags SET aliases = @aliases WHERE id = @id", conn, transaction);
		var aliasesParameter = updateReciprocal.Parameters.Add("@aliases", System.Data.DbType.String);
		var idParameter = updateReciprocal.Parameters.Add("@id", System.Data.DbType.Int32);

		foreach (int id in aliasGroup) {
			aliasesParameter.Value = SerializeAliasIds(aliasGroup.Where(relatedId => relatedId != id));
			idParameter.Value = id;
			updateReciprocal.ExecuteNonQuery();
		}

		transaction.Commit();
	}

	private static HashSet<int> ParseAliasIds (string? aliases)
		=> [.. ConvertUtils.StringToIntList(aliases ?? "")];

	private static object SerializeAliasIds (IEnumerable<int> aliases) {
		List<int> ids = aliases.Where(id => id > 0).Distinct().OrderBy(id => id).ToList();
		return ids.Count == 0 ? DBNull.Value : string.Join(" ", ids);
	}

	// ➖ Remove a tag
	public static void RemoveTag (int id) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "DELETE FROM Tags WHERE id = @id";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", id);

		cmd.ExecuteNonQuery();
	}

	// 📏 Total number of tags
	public static int GetCount () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT COUNT(*) FROM Tags";

		using var cmd = new SQLiteCommand(sql, conn);
		return Convert.ToInt32(cmd.ExecuteScalar());
	}

	// Dictionary to store the usage count of all tags
	private static Dictionary<int, int>? DictionaryAllTagsUsage;

	// 📏 Count the usage of all tags in posts and store it in a dictionary
	public static void CountTagsUsage () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT tags FROM Posts";
		using var cmd = new SQLiteCommand(sql, conn);
		using var reader = cmd.ExecuteReader();

		var counts = new Dictionary<int, int>();

		while (reader.Read()) {
			var tags = ConvertUtils.StringToIntList(reader["tags"].ToString()!);

			foreach (var tag in tags) {
				if (counts.TryGetValue(tag, out int value))
					counts[tag] = ++value;
				else
					counts[tag] = 1;
			}
		}

		DictionaryAllTagsUsage = counts;
	}

	// 📏 Number of times a tag is used in posts
	public static int GetTagUsage (int tagId) {
		if (DictionaryAllTagsUsage == null) CountTagsUsage();
		return DictionaryAllTagsUsage?.TryGetValue(tagId, out int count) ?? false ? count : 0;
	}

	// 📖 Retrieve a tag by its ID
	public static Tag GetTag (int id) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Tags WHERE id = @id";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", id);

		using var reader = cmd.ExecuteReader();

		if (!reader.Read()) return null!;

		return new Tag(
			id, 
			reader["name"].ToString() ?? "Tag", 
			reader["specificTags"].ToString() ?? "Tag", 
			reader["description"].ToString(),
			reader["obsolete"].ToString() ?? "0",
			reader["aliases"] == DBNull.Value ? null : reader["aliases"].ToString()
			);
	}

	// 📖 Retrieve all tags
	public static List<Tag> GetAllTags () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Tags";

		using var cmd = new SQLiteCommand(sql, conn);
		using var reader = cmd.ExecuteReader();

		var list = new List<Tag>();

		while (reader.Read()) {
			list.Add(new Tag (
				Convert.ToInt32(reader["id"]), 
				reader["name"].ToString() ?? "Tag", 
				reader["specificTags"].ToString() ?? "Tag",
				reader["description"].ToString(),
				reader["obsolete"].ToString() ?? "0",
				reader["aliases"] == DBNull.Value ? null : reader["aliases"].ToString()
			));
		}

		return list;
	}

	// Dictionary to cache tag IDs by their names for quick lookup
	private static Dictionary<string, int>? _tagIdCache;

	// 📖 Retrieve a tag's ID from its name
	public static int GetTagIdByName (string name) {
		if (_tagIdCache == null) LoadTagIdCache();
		return _tagIdCache?.TryGetValue(name, out var id) ?? false ? id : -1;
	}

	// Load the tag ID cache from the database
	public static void LoadTagIdCache () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT id, name FROM Tags";
		using var cmd = new SQLiteCommand(sql, conn);
		using var reader = cmd.ExecuteReader();

		_tagIdCache = [];

		while (reader.Read()) {
			_tagIdCache[reader["name"].ToString()!] = Convert.ToInt32(reader["id"]);
		}
	}
}

public class Tag (int id, string name, string specificTags, string? description, string obsolete = "0", string? aliases = null) {
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
	public string SpecificTags { get; set; } = specificTags;
	public string? Description { get; set; } = description;
	public string Obsolete { get; set; } = obsolete;
	public string? Aliases { get; set; } = aliases;

	public int Count =>  TagsManager.GetTagUsage(Id);
}

