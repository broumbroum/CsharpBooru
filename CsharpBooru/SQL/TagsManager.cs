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
			INSERT INTO Tags (name, specificTags)
			VALUES (@name, @specificTags);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@specificTags", specificTags);

		cmd.ExecuteNonQuery();
	}

	// ➕ Add a tag or retrieve its ID if it already exists
	public static int GetOrCreateTag (string name) {
		using SQLiteConnection? conn = DataBase.GetConnection();
		conn.Open();

		// 1. Vérifier si le tag existe déjà
		string checkSql = "SELECT id FROM Tags WHERE name = @name";

		using (var checkCmd = new SQLiteCommand(checkSql, conn)) {
			checkCmd.Parameters.AddWithValue("@name", name);

			var result = checkCmd.ExecuteScalar();
			if (result != null) {
				return Convert.ToInt32(result);
			}
		}

		// 2. Le tag n'existe pas → on le crée avec specificTags = "Tag"
		string sql = @"
			INSERT INTO Tags (name, specificTags)
			VALUES (@name, @specificTags);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", name);
		cmd.Parameters.AddWithValue("@specificTags", "Tag");

		cmd.ExecuteNonQuery();

		// 3. Récupérer l'ID du nouveau tag
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
				specificTags = @specificTags
			WHERE id = @id;
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", tag.Id);
		cmd.Parameters.AddWithValue("@name", tag.Name);
		cmd.Parameters.AddWithValue("@specificTags", tag.SpecificTags);

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

	// 📏 Number of times a tag is used in posts
	public static int CountTagUsage (int tagId) {
		int count = 0;

		foreach (var post in PostsManager.GetAllPosts()) {
			if (post.Tags.Contains(tagId))
				count++;
		}

		return count;
	}

	// 📖 Retrieve a tag by its ID
	public static Tag GetTag (int id) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Tags WHERE id = @id";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", id);

		using var reader = cmd.ExecuteReader();

		if (!reader.Read())
			return null;

		return new Tag(id, reader["name"].ToString(), reader["specificTags"].ToString());
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
				reader["name"].ToString(), 
				reader["specificTags"].ToString())
			);
		}

		return list;
	}

	// 📖 Retrieve a tag's ID from its name
	public static int? GetTagIdByName (string name) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT id FROM Tags WHERE name = @name";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", name);

		var result = cmd.ExecuteScalar();
		if (result == null)
			return null;

		return Convert.ToInt32(result);
	}
}

public class Tag (int id, string name, string specificTags) {
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
	public string SpecificTags { get; set; } = specificTags;

	public int Count =>  TagsManager.CountTagUsage(Id);
}

