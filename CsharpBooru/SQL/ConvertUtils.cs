using System.Collections.Generic;
using System.Linq;

namespace CsharpBooru.SQL;
public static class ConvertUtils {
	public static string IntListToString (List<int> list)
		=> list == null || list.Count == 0 ? " " : string.Join(" ", list);

	public static List<int> StringToIntList (string s)
		=> string.IsNullOrWhiteSpace(s)
			? new List<int>()
			: s.Split(' ').Select(int.Parse).ToList();

	public static string StringListToString (List<string> list)
		=> list == null || list.Count == 0 ? "" : string.Join(" ", list);

	public static List<string> StringToStringList (string s)
		=> string.IsNullOrWhiteSpace(s)
			? new List<string>()
			: s.Split(' ').ToList();
}
