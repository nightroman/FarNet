namespace RightWords;

static class Text
{
	static string GetString(string name) => TheHost.Instance.GetString(name)!;

	public static string AddToDictionary => GetString("AddToDictionary");
	public static string DoIgnore => GetString("DoIgnore");
	public static string DoIgnoreAll => GetString("DoIgnoreAll");
	public static string DoAddToDictionary => GetString("DoAddToDictionary");
	public static string DoCorrectWord => GetString("DoCorrectWord");
	public static string DoCorrectText => GetString("DoCorrectText");
	public static string Common => GetString("Common");
	public static string NewWord => GetString("NewWord");
	public static string SampleStem => GetString("SampleStem");
}
