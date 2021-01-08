
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

namespace FarNet.RightWords
{
	static class My
	{
		public const string GuidString = "ca7ecdc0-f446-4bff-a99d-06c90fe0a3a9";
		#region private
		static readonly IModuleManager Manager = Far.Api.GetModuleManager(Settings.ModuleName);
		static string GetString(string name) => Manager.GetString(name);
		#endregion
		#region help
		public static string AddToDictionaryHelp => Far.Api.GetHelpTopic("add-to-dictionary");
		#endregion
		public static string AddToDictionary => GetString("AddToDictionary");
		public static string DoIgnore => GetString("DoIgnore");
		public static string DoIgnoreAll => GetString("DoIgnoreAll");
		public static string DoAddToDictionary => GetString("DoAddToDictionary");
		public static string Word => GetString("Word");
		public static string Thesaurus => GetString("Thesaurus");
		public static string DoCorrectWord => GetString("DoCorrectWord");
		public static string DoCorrectText => GetString("DoCorrectText");
		public static string DoThesaurus => GetString("DoThesaurus");
		public static string Common => GetString("Common");
		public static string Searching => GetString("Searching");
		public static string NewWord => GetString("NewWord");
		public static string ExampleStem => GetString("ExampleStem");
		public static string ExampleStem2 => GetString("ExampleStem");
	}
}
