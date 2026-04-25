using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsharpBooru.Setting;
internal static class SettingFile {

	private static readonly string configPath = Path.Combine(AppContext.BaseDirectory, "setting.config");

	internal static readonly Dictionary<string, string> _values = new();

	public static bool Load () {
		if (File.Exists(configPath) == false) {
			return false;
		}

		foreach (var line in File.ReadAllLines(configPath)) {

			if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
			
			var parts = line.Split('=', 2); 
			if (parts.Length != 2) continue;

			string key = parts[0].Trim();
			string value = parts[1].Trim();
			_values[key] = value;
		}

		return true;

	}

	public static void Save () {
		var lines = _values.Select(kvp => $"{kvp.Key}={kvp.Value}");
		File.WriteAllLines(configPath, lines);
	}

	public static void Create (string path) {
		_values["Version"] = "1";
		_values["Path"] = path;
		_values["PostPerPage"] = "20";
		_values["FillThumbnailPost"] = "false";

		Save();
	}
}
