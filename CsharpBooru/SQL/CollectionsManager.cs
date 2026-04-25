using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CsharpBooru.SQL;

public static class CollectionsManager {

	// ➕ Add a collection
	public static void AddCollection (Collection collection) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = @"
			INSERT INTO Collections (name, posts)
			VALUES (@name, @posts);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@name", collection.Name);
		cmd.Parameters.AddWithValue("@posts", ConvertUtils.IntListToString(collection.Posts));

		cmd.ExecuteNonQuery();
	}

	// ✏️ Edit a collection
	public static void UpdateCollection (Collection collection) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = @"
			UPDATE Collections
			SET name = @name,
				posts = @posts
			WHERE id = @id;
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", collection.Id);
		cmd.Parameters.AddWithValue("@name", collection.Name);
		cmd.Parameters.AddWithValue("@posts", ConvertUtils.IntListToString(collection.Posts));

		cmd.ExecuteNonQuery();
	}

	// ➖ Delete a collection
	public static void RemoveCollection (int id) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "DELETE FROM Collections WHERE id = @id";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", id);
		cmd.ExecuteNonQuery();

		// Réorganiser les IDs
		ReorderCollectionsAfterDelete(id);
	}

	// 📏 Total number of collections
	public static int GetCount () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT COUNT(*) FROM Collections";

		using var cmd = new SQLiteCommand(sql, conn);
		return Convert.ToInt32(cmd.ExecuteScalar());
	}

	// 🔄 Reorganize collection IDs after a collection is deleted
	private static void ReorderCollectionsAfterDelete (int deletedId) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		// Décaler tous les IDs supérieurs
		string sql = @"
        UPDATE Collections
        SET id = id - 1
        WHERE id > @deletedId;
    ";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@deletedId", deletedId);
		cmd.ExecuteNonQuery();
	}

	// 🔄 Update the post lists in the collections after a post is deleted
	internal static void UpdateCollectionsAfterPostDelete (SQLiteConnection conn, int deletedPostId) {
		string selectSql = "SELECT id, posts FROM Collections";

		using var selectCmd = new SQLiteCommand(selectSql, conn);
		using var reader = selectCmd.ExecuteReader();

		var updates = new List<(int collectionId, string newPosts)>();

		while (reader.Read()) {
			int collectionId = Convert.ToInt32(reader["id"]);
			string postsStr = reader["posts"]?.ToString() ?? "";

			var list = ConvertUtils.StringToIntList(postsStr);

			// Retirer l'ID supprimé
			list.Remove(deletedPostId);

			// Décrémenter les IDs supérieurs
			for (int i = 0; i < list.Count; i++) {
				if (list[i] > deletedPostId)
					list[i]--;
			}

			updates.Add((collectionId, ConvertUtils.IntListToString(list)));
		}

		// Appliquer les mises à jour
		foreach (var u in updates) {
			string updateSql = "UPDATE Collections SET posts = @posts WHERE id = @id";
			using var updateCmd = new SQLiteCommand(updateSql, conn);
			updateCmd.Parameters.AddWithValue("@posts", u.newPosts);
			updateCmd.Parameters.AddWithValue("@id", u.collectionId);
			updateCmd.ExecuteNonQuery();
		}
	}

	// 📖 Retrieve a collection by its ID
	public static Collection GetCollection (int id) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Collections WHERE id = @id";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", id);

		using var reader = cmd.ExecuteReader();

		if (!reader.Read())
			return null;

		return new Collection(
			Convert.ToInt32(reader["id"]), 
			reader["name"].ToString(), 
			ConvertUtils.StringToIntList(reader["posts"].ToString())
			);
	}

	// 📖 Retrieve all collections
	public static List<Collection> GetAllCollections () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Collections";

		using var cmd = new SQLiteCommand(sql, conn);
		using var reader = cmd.ExecuteReader();

		var list = new List<Collection>();

		while (reader.Read()) {
			list.Add(new
				(Convert.ToInt32(reader["id"]), 
				reader["name"].ToString(),
				ConvertUtils.StringToIntList(reader["posts"].ToString()))
				);
		}

		return list;
	}

}

public class Collection (int id, string name, List<int> posts) {
	public int Id { get; set; } = id;
	public string Name { get; set; } = name;
	public List<int> Posts { get; set; } = posts;
}
