﻿using EditorConfig.Core;
using FarNet;

namespace EditorKit;

[ModuleEditor(Name = "EditorConfig", Mask = "*", Id = "961AC212-9A24-44B4-9E61-24818971457D")]
public class Editor : ModuleEditor
{
	const string key_trim_trailing_whitespace = "trim_trailing_whitespace";
	const string key_insert_final_newline = "insert_final_newline";
	const string key_indent_style = "indent_style";
	const string key_indent_size = "indent_size";
	const string key_charset = "charset";

	bool do_trim_trailing_whitespace;
	bool do_insert_final_newline;

	static void Extras(IEditor editor, string fileName)
	{
		var settings = Settings.Default.GetData();

		var colorerType = settings.ColorerTypes.FirstOrDefault(x => Far.Api.IsMaskMatch(fileName, x.Mask, x.Full));
		if (colorerType is { })
			editor.Opened += (s, e) => SetColorerType(colorerType.Type);
	}

	static void SetColorerType(string colorerType)
	{
		if (colorerType.Any(x => !Char.IsLetterOrDigit(x)))
			throw new ModuleException($"Invalid Colorer type: '{colorerType}'.");

		Far.Api.PostMacro($"Plugin.Call('D2F36B62-A470-418d-83A3-ED7A3710E5B5', 'Types', 'Set', '{colorerType}')");
	}

	public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
	{
		// get the file name, it may be amended
		var fileName = editor.FileName;

		// apply extras from settings
		Extras(editor, fileName);

		// get configurations
		var parser = new EditorConfigParser();
		var configurations = parser.GetConfigurationFilesTillRoot(fileName);
		if (configurations.Count == 0)
			return;

		// get file properties
		var config = parser.Parse(fileName, configurations);
		var properties = config.Properties;

		if (properties.TryGetValue(key_trim_trailing_whitespace, out var trim) && string.Equals(trim, "true", StringComparison.OrdinalIgnoreCase))
		{
			do_trim_trailing_whitespace = true;
		}

		if (properties.TryGetValue(key_insert_final_newline, out var eof) && string.Equals(eof, "true", StringComparison.OrdinalIgnoreCase))
		{
			do_insert_final_newline = true;
		}

		if (do_trim_trailing_whitespace || do_insert_final_newline)
		{
			editor.Saving += OnSaving;
		}

		if (properties.TryGetValue(key_indent_style, out var style))
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

		if (properties.TryGetValue(key_indent_size, out var size))
		{
			if (int.TryParse(size, out var value) && value > 0)
			{
				editor.TabSize = value;
			}
		}

		if (properties.TryGetValue(key_charset, out var charset))
		{
			var codePage = editor.CodePage;
			switch (charset.ToLower())
			{
				case "utf-8":
					if (editor.CodePage != 65001)
						editor.CodePage = 65001;
					if (editor.WriteByteOrderMark)
						editor.WriteByteOrderMark = false;
					break;
				case "utf-8-bom":
					if (editor.CodePage != 65001)
						editor.CodePage = 65001;
					if (!editor.WriteByteOrderMark)
						editor.WriteByteOrderMark = true;
					break;
				case "utf-16le":
					if (editor.CodePage != 1200)
						editor.CodePage = 1200;
					editor.WriteByteOrderMark = true;
					break;
				case "utf-16be":
					if (editor.CodePage != 1201)
						editor.CodePage = 1201;
					editor.WriteByteOrderMark = true;
					break;
			}
		}
	}

	// Trims lines, etc.
	void OnSaving(object? sender, EventArgs e)
	{
		var editor = (IEditor)sender!;

		if (do_trim_trailing_whitespace)
		{
			foreach (ILine line in editor.Lines)
			{
				var s1 = line.Text2;
				var s2 = s1.TrimEnd();
				if (s1.Length != s2.Length)
					line.Text2 = s2;
			}
		}

		if (do_insert_final_newline)
		{
			var line = editor[^1];
			if (line.Length > 0)
			{
				var frame = editor.Frame;
				editor.Add(string.Empty);
				editor.Frame = frame;
			}
		}
	}
}
