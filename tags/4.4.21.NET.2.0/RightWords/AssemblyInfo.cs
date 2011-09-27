﻿
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("http://code.google.com/p/farnet/")]
[assembly: AssemblyCopyright("Copyright (c) 2011 Roman Kuzmin")]
[assembly: AssemblyDescription("Spell-checker and thesaurus")]
[assembly: AssemblyProduct("FarNet.RightWords")]
[assembly: AssemblyTitle("FarNet module RightWords")]
[assembly: AssemblyVersion("1.0.9")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(true)]

namespace FarNet.RightWords
{
	static class UI
	{
		#region private
		static IModuleManager Manager = Far.Net.GetModuleManager(typeof(UI));
		static string GetString(string name) { return Manager.GetString(name); }
		#endregion
		static public string AddToDictionary { get { return GetString("AddToDictionary"); } }
		static public string DoIgnore { get { return GetString("DoIgnore"); } }
		static public string DoIgnoreAll { get { return GetString("DoIgnoreAll"); } }
		static public string DoAddToDictionary { get { return GetString("DoAddToDictionary"); } }
		static public string Word { get { return GetString("Word"); } }
		static public string Thesaurus { get { return GetString("Thesaurus"); } }
		static public string DoCorrectWord { get { return GetString("DoCorrectWord"); } }
		static public string DoCorrectText { get { return GetString("DoCorrectText"); } }
		static public string DoHighlighting { get { return GetString("DoHighlighting"); } }
		static public string DoThesaurus { get { return GetString("DoThesaurus"); } }
		static public string Common { get { return GetString("Common"); } }
		static public string Searching { get { return GetString("Searching"); } }
		static public string NewWord { get { return GetString("NewWord"); } }
		static public string ExampleStem { get { return GetString("ExampleStem"); } }
	}
}