using System;
using System.Data.SQLite;
using System.IO;

namespace CsharpBooru.SQL;
internal class DataBase {

    public static string path = "";
	private const string files = "files/", thumbnails = "thumbnails/";

	public static string PathBooru { get => Path.Combine(path, "Booru.db").Replace("\\", "/"); }
	public static string FilesPath { get => Path.Combine(path, files).Replace("\\", "/"); }
	public static string ThumbnailsPath { get => Path.Combine(path, thumbnails).Replace("\\", "/"); }

	internal static SQLiteConnection GetConnection () {
		if (string.IsNullOrWhiteSpace(path))
			throw new InvalidOperationException("???");
		return new SQLiteConnection($"Data Source=" + PathBooru + "");
	}

	public static void CreateSQL () {
        System.Diagnostics.Debug.WriteLine("Création de la base de données SQLite...");
		SQLiteConnection.CreateFile(PathBooru);

        using (var conn = new SQLiteConnection($"Data Source=" + PathBooru + "")) {
            System.Diagnostics.Debug.WriteLine("Connexion à la base de données SQLite...");
			conn.Open();

            string sql = @"
    CREATE TABLE IF NOT EXISTS Posts (
        id INTEGER PRIMARY KEY,
        filename TEXT NOT NULL,
        tags TEXT,
        sources TEXT,
        related TEXT,
        rating TEXT,
        note TEXT
    );

    CREATE TABLE IF NOT EXISTS Tags (
        id INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        specificTags TEXT
    );

    CREATE TABLE IF NOT EXISTS Collections (
        id INTEGER PRIMARY KEY,
        name TEXT NOT NULL,
        posts TEXT
    );
";
            using (var cmd = new SQLiteCommand(sql, conn)) {
                System.Diagnostics.Debug.WriteLine("Exécution de la requête SQL pour créer les tables...");
				cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Base SQLite créée avec succès !");
        }
        System.Diagnostics.Debug.WriteLine("Base de données SQLite créée avec succès !");
	}

	private static bool ColumnExists (string table, string column) {
		using var conn = DataBase.GetConnection();
		conn.Open();

		using var cmd = new SQLiteCommand($"PRAGMA table_info({table});", conn);
		using var reader = cmd.ExecuteReader();

		while (reader.Read()) {
			string colName = reader["name"].ToString();
			if (string.Equals(colName, column, StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private static void EnsureTagColumns () {
		using var conn = DataBase.GetConnection();
		conn.Open();

		// specificTags
		if (!ColumnExists("Tags", "specificTags")) {
			using var cmd = new SQLiteCommand(
				"ALTER TABLE Tags ADD COLUMN specificTags TEXT DEFAULT 'Tag';", conn);
			cmd.ExecuteNonQuery();
		}

		// description
		if (!ColumnExists("Tags", "description")) {
			using var cmd = new SQLiteCommand(
				"ALTER TABLE Tags ADD COLUMN description TEXT DEFAULT NULL;", conn);
			cmd.ExecuteNonQuery();
		}
	}

	public static void DataBaseCheck () {
		EnsureTagColumns();
	}
}
