
// EditorKit: a few editor tools
// Author: Roman Kuzmin

using FarNet;
using System;

// Trims line ends on saving a file.
[System.Runtime.InteropServices.Guid("7a42d03a-83d3-4c8f-b4d3-6483bf1acaf0")]
[ModuleEditor(Name = TrimSaving.Name)]
public class TrimSaving : ModuleEditor
{
	public const string Name = "TrimSaving";

	// Called when a file is opened. It installs OnSaving().
	public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
	{
		editor.Saving += OnSaving;
	}

	// Trims line ends.
	// A few editor performance tricks:
	// *) do not set a line text if it is not actually changed;
	// *) use faster (object)string comparison when possible.
	void OnSaving(object sender, EventArgs e)
	{
		IEditor editor = (IEditor)sender;
		foreach(ILine line in editor.Lines)
		{
			string s1 = line.Text;
			string s2 = s1.TrimEnd();
			if ((object)s1 != (object)s2)
				line.Text = s2;
		}
	}
}

// Sets tabs expansion mode to 'All' for some files.
[System.Runtime.InteropServices.Guid("c0d595e8-bb33-4724-ade0-4a659488004a")]
[ModuleEditor(Name = "ExpandTabsAll", Mask = "*.fs;*.fsi;*.fsx;*.fsscript")]
public class ExpandTabsAll : ModuleEditor
{
	public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
	{
		editor.ExpandTabs = ExpandTabsMode.All;
	}
}

// Sets tabs expansion mode to 'All' and tab size to 2 for some files.
[System.Runtime.InteropServices.Guid("0050825a-d915-4ffc-8fb4-e723a456c5a2")]
[ModuleEditor(Name = "ExpandTabsAll2", Mask = "*.xml;*.*proj")]
public class ExpandTabsAll2 : ModuleEditor
{
	public override void Invoke(IEditor editor, ModuleEditorEventArgs e)
	{
		editor.ExpandTabs = ExpandTabsMode.All;
		editor.TabSize = 2;
	}
}
