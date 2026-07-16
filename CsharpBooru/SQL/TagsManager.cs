using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CsharpBooru.SQL;
public static class TagsManager {

	// ➕ Add a tag
	public static void AddTag (string name, string specificTags) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = @"
			INSERT INTO Tags (name, specificTags, description)
			VALUES (@name, @specificTags, @description);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@specificTags", specificTags);
		cmd.Parameters.AddWithValue("@description", DBNull.Value);

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

		// 2. The tag does not exist → create it with specificTags = "Tag"
		string sql = @"
			INSERT INTO Tags (name, specificTags, description)
			VALUES (@name, @specificTags, @description);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@specificTags", "Tag");
		cmd.Parameters.AddWithValue("@description", DBNull.Value);

		cmd.ExecuteNonQuery();

		// 3. Retrieve the ID of the new tag
		string idSql = "SELECT last_insert_rowid();";

		using (var idCmd = new SQLiteCommand(idSql, conn)) {
			return Convert.ToInt32(idCmd.ExecuteScalar());
		}
	}

	// ✏️ Edit a tag
	public static void UpdateTag (Tag tag) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = @"
			UPDATE Tags
			SET name = @name,
				specificTags = @specificTags,
				description = @description
			WHERE id = @id;
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", tag.Id);
		cmd.Parameters.AddWithValue("@name", tag.Name);
		cmd.Parameters.AddWithValue("@specificTags", tag.SpecificTags);
		cmd.Parameters.AddWithValue("@description", (object?)tag.Description ?? DBNull.Value);

		cmd.ExecuteNonQuery();
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
				if (counts.ContainsKey(tag))
					counts[tag]++;
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
			reader["description"].ToString()
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
				reader["description"].ToString())
			);
		}

		return list;
	}

	// 📖 Retrieve a tag's ID from its name
	public static int GetTagIdByName (string name) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT id FROM Tags WHERE name = @name";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", name);

		var result = cmd.ExecuteScalar();
		if (result == null)
			return -1;

		return Convert.ToInt32(result);
	}
}

public class Tag (int id, string name, string specificTags, string? description) {
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
	public string SpecificTags { get; set; } = specificTags;
	public string? Description { get; set; } = description;

	public int Count =>  TagsManager.GetTagUsage(Id);
}

