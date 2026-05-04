using System;

namespace CsharpBooru.Setting;
public static class SettingValue {

	public static int Version {
		get {
			SettingFile._values.TryGetValue("Version", out string? value);
			if (value == null) return 1;
			return Convert.ToInt32(value);
		}
	}

	public static string? Path {
		get {
			SettingFile._values.TryGetValue("Path", out string? value);
			return value + "\\";
		}
	}

	public static int PostPerPage { 
		get {
			SettingFile._values.TryGetValue("PostPerPage", out string? value);
			if (value == null) {
				SettingFile._values["PostPerPage"] = "20";
				return 20;
			}
			return Convert.ToInt32(value);
		}set { 
			SettingFile._values["PostPerPage"] = value.ToString();
		}
	}

	public static bool FillThumbnailPost {
		get {
			SettingFile._values.TryGetValue("FillThumbnailPost", out string? value);
			if (value == null) { 
				SettingFile._values["FillThumbnailPost"] = "false";
				return false; 
			}
			return Convert.ToBoolean(value);
		} set {
			SettingFile._values["FillThumbnailPost"] = value.ToString();
		}
	}

	public static string OrderTag {
		get {
			SettingFile._values.TryGetValue("OrderTag", out string? value);
			return value ?? "Alphabetical Order";
		} set {
			SettingFile._values["OrderTag"] = value;
		}
	}
}
