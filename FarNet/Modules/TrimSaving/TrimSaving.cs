
using FarNet;
using System;
using System.Runtime.InteropServices;

// Trims line ends in a saving file.
[Guid("7a42d03a-83d3-4c8f-b4d3-6483bf1acaf0")]
[ModuleEditor(Name = TrimSaving.Name)]
public class TrimSaving : ModuleEditor
{
	public const string Name = "TrimSaving";

	// Called when a file is opened. It installs OnSaving().
	public override void Invoke(object sender, ModuleEditorEventArgs e)
	{
		IEditor editor = (IEditor)sender;
		editor.Saving += OnSaving;
	}

	// Trims line ends.
	// A few editor performance tricks:
	// *) use Begin() and End() methods for batch line operations;
	// *) do not set a line text if it is not actually changed;
	// *) use faster (object)string comparison when possible.
	void OnSaving(object sender, EventArgs e)
	{
		IEditor editor = (IEditor)sender;
		editor.Begin();
		foreach(ILine line in editor.Lines)
		{
			string s1 = line.Text;
			string s2 = s1.TrimEnd();
			if ((object)s1 != (object)s2)
				line.Text = s2;
		}
		editor.End();
	}
}
