using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace CsharpBooru.SQL;
public static class PostsManager {

	// ➕ Add a post
	public static void AddPost (Post post) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = @"
			INSERT INTO Posts (filename, tags, sources, related, note, rating)
			VALUES (@filename, @tags, @sources, @related, @note, @rating);
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@filename", post.Filename);
		cmd.Parameters.AddWithValue("@tags", ConvertUtils.IntListToString(post.Tags));
		cmd.Parameters.AddWithValue("@sources", ConvertUtils.StringListToString(post.Sources));
		cmd.Parameters.AddWithValue("@related", ConvertUtils.IntListToString(post.Related));
		cmd.Parameters.AddWithValue("@note", post.Note);
		cmd.Parameters.AddWithValue("@rating", post.Rating);

		cmd.ExecuteNonQuery();
	}

	// ➖ Remove a post
	public static void RemovePost (int id) {
		string? filename = GetPost(id)?.Filename;

		using var conn = DataBase.GetConnection();
		conn.Open();

		using var transaction = conn.BeginTransaction();

		// 1. Supprimer le post
		string deleteSql = "DELETE FROM Posts WHERE id = @id";
		using (var deleteCmd = new SQLiteCommand(deleteSql, conn)) {
			deleteCmd.Parameters.AddWithValue("@id", id);
			deleteCmd.ExecuteNonQuery();
		}

		// 2. Mettre à jour les related dans Posts
		UpdateRelatedAfterPostDelete(conn, id);

		// 3. Mettre à jour les Collections (colonne Posts)
		CollectionsManager.UpdateCollectionsAfterPostDelete(conn, id);

		// 4. Réordonner les IDs des Posts
		string reorderSql = @"
        UPDATE Posts
        SET id = id - 1
        WHERE id > @deletedId;
    ";

		using (var reorderCmd = new SQLiteCommand(reorderSql, conn)) {
			reorderCmd.Parameters.AddWithValue("@deletedId", id);
			reorderCmd.ExecuteNonQuery();
		}

		transaction.Commit();

		if (filename != null) FileManager.DeleteFile(filename);
	}

	// 🔄 Update related posts after a post is deleted
	private static void UpdateRelatedAfterPostDelete (SQLiteConnection conn, int deletedPostId) {
		string selectSql = "SELECT id, related FROM Posts";

		using var selectCmd = new SQLiteCommand(selectSql, conn);
		using var reader = selectCmd.ExecuteReader();

		var updates = new List<(int postId, string newRelated)>();

		while (reader.Read()) {
			int postId = Convert.ToInt32(reader["id"]);
			string relatedStr = reader["related"]?.ToString() ?? "";

			var list = ConvertUtils.StringToIntList(relatedStr);

			// 1. Retirer l'ID supprimé
			list.Remove(deletedPostId);

			// 2. Décrémenter les IDs supérieurs
			for (int i = 0; i < list.Count; i++) {
				if (list[i] > deletedPostId)
					list[i]--;
			}

			updates.Add((postId, ConvertUtils.IntListToString(list)));
		}

		// Appliquer les mises à jour
		foreach (var u in updates) {
			string updateSql = "UPDATE Posts SET related = @related WHERE id = @id";
			using var updateCmd = new SQLiteCommand(updateSql, conn);
			updateCmd.Parameters.AddWithValue("@related", u.newRelated);
			updateCmd.Parameters.AddWithValue("@id", u.postId);
			updateCmd.ExecuteNonQuery();
		}
	}

	// ✏️ Edit a post
	public static void UpdatePost (Post post) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = @"
			UPDATE Posts
			SET filename = @filename,
				tags = @tags,
				sources = @sources,
				related = @related,
				note = @note,
				rating = @rating
			WHERE id = @id;
		";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", post.Id);
		cmd.Parameters.AddWithValue("@filename", post.Filename);
		cmd.Parameters.AddWithValue("@tags", ConvertUtils.IntListToString(post.Tags));
		cmd.Parameters.AddWithValue("@sources", ConvertUtils.StringListToString(post.Sources));
		cmd.Parameters.AddWithValue("@related", ConvertUtils.IntListToString(post.Related));
		cmd.Parameters.AddWithValue("@note", post.Note);
		cmd.Parameters.AddWithValue("@rating", post.Rating);

		cmd.ExecuteNonQuery();
	}

	// 📏 Retrieve the size of the array
	public static int GetCount () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT COUNT(*) FROM Posts";

		using var cmd = new SQLiteCommand(sql, conn);
		return Convert.ToInt32(cmd.ExecuteScalar());
	}

	// 📖 Retrieve a post
	public static Post GetPost (int id) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Posts WHERE id = @id";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@id", id);

		using var reader = cmd.ExecuteReader();

		if (!reader.Read())
			return null; // Aucun post trouvé
		return new(
			Convert.ToInt32(reader["id"]),
			reader["filename"].ToString(),
			ConvertUtils.StringToIntList(reader["tags"].ToString()),
			ConvertUtils.StringToStringList(reader["sources"].ToString()),
			ConvertUtils.StringToIntList(reader["related"].ToString()),
			reader["note"].ToString(),
			reader["rating"].ToString()
			);
	}

	// 📖 Retrieve all posts
	public static List<Post> GetAllPosts () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT * FROM Posts";

		using var cmd = new SQLiteCommand(sql, conn);
		using var reader = cmd.ExecuteReader();

		var list = new List<Post>();

		while (reader.Read()) {
			list.Add(new(
				Convert.ToInt32(reader["id"]),
				reader["filename"].ToString(),
				ConvertUtils.StringToIntList(reader["tags"].ToString()),
				ConvertUtils.StringToStringList(reader["sources"].ToString()),
				ConvertUtils.StringToIntList(reader["related"].ToString()),
				reader["note"].ToString(),
				reader["rating"].ToString()
			));
		}

		return list;
	}

	// 🔍 Check if a filename already exists
	public static bool FilenameExists (string filename) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		string sql = "SELECT COUNT(*) FROM Posts WHERE filename = @filename";

		using var cmd = new SQLiteCommand(sql, conn);
		cmd.Parameters.AddWithValue("@filename", filename);

		long count = (long)cmd.ExecuteScalar();
		return count > 0;
	}

}

public class Post (int id, string filename, List<int> tags, List<string> sources, List<int> related, string note, string rating) {
	public int Id { get; set; } = id;
	public string Filename { get; set; } = filename;
	public List<int> Tags { get; set; } = tags;
	public List<string> Sources { get; set; } = sources;
	public List<int> Related { get; set; } = related;
	public string Note { get; set; } = note;
	public string Rating { get; set; } = rating;
}

