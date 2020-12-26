
// FarNet module EditorKit
// Copyright (c) Roman Kuzmin

using EditorConfig.Core;
using FarNet;
using System;
using System.Collections.Generic;
using System.IO;

[System.Runtime.InteropServices.Guid("961AC212-9A24-44B4-9E61-24818971457D")]
[ModuleEditor(Name = "EditorConfig", Mask = "*")]
public class Config : ModuleEditor
{
	const string trim_trailing_whitespace = "trim_trailing_whitespace";
	const string indent_style = "indent_style";
	const string indent_size = "indent_size";

	// Called by the core when a file matching Mask is opened in the editor.
	public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
	{
		// get the file name, it may change to the profile path
		var fileName = editor.FileName;

		// get the usual configurations
		var parser = new EditorConfigParser();
		var configurations = parser.GetConfigurationFilesTillRoot(fileName);
		if (configurations.Count == 0)
		{
			// get the profile configurations to use with the amended file
			var defaults = GetProfileConfigurations(ref fileName);
			if (defaults == null)
				return;

			configurations = defaults;
		}

		// get the properties for the original or amended file
		var config = parser.Parse(fileName, configurations);
		var properties = config.Properties;

		if (properties.TryGetValue(trim_trailing_whitespace, out var trim) && trim == "true")
		{
			editor.Saving += OnSaving;
		}

		if (properties.TryGetValue(indent_style, out var style))
		{
			switch (style)
			{
				case "tab":
					editor.ExpandTabs = ExpandTabsMode.None;
					break;
				case "space":
					editor.ExpandTabs = editor.DisableHistory ? ExpandTabsMode.New : ExpandTabsMode.All;
					break;
			}
		}

		if (properties.TryGetValue(indent_size, out var size))
		{
			if (int.TryParse(size, out var value) && value > 0)
			{
				editor.TabSize = value;
			}
		}
	}

	// Trims line ends.
	void OnSaving(object sender, EventArgs e)
	{
		var editor = (IEditor)sender;
		foreach (ILine line in editor.Lines)
		{
			string s1 = line.Text;
			string s2 = s1.TrimEnd();
			if (!ReferenceEquals(s1, s2))
				line.Text = s2;
		}
	}

	// Gets profile configurations and alters the file name.
	IList<EditorConfigFile> GetProfileConfigurations(ref string fileName)
	{
		var root = Manager.GetFolderPath(SpecialFolder.RoamingData, false);
		var path = Path.Combine(root, ".editorconfig");
		if (!File.Exists(path))
			return null;

		fileName = Path.Combine(root, Path.GetFileName(fileName));

		var configFile = new EditorConfigFile(path);
		return new EditorConfigFile[] { configFile };
	}
}
