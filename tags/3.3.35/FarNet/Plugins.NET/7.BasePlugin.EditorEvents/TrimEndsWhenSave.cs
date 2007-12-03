
// TrimEndsWhenSave: example of BasePlugin and editor events
// *) Connect() adds OnAfterOpen() for all files opened in the editor.
// *) OnAfterOpen() checks file types and adds OnBeforeSave() for .txt files.
// *) OnBeforeSave() trims all line ends.
// *) Disconnect removes OnAfterOpen().
// *) A few tricks:
// 1) Begin\End editor methods drastically improve performance for large files;
// 2) using (object)string for comparison in this case is faster than s1 != s2;
// 3) comparison avoids setting of same text to lines.

// If you want to use it for all files then code is even simpler: remove
// OnAfterOpen() and use OnBeforeSave() handler in Connect(), Disconnect().

using System;
using FarManager;

public class TrimEndsWhenSave : BasePlugin
{
	public override void Connect()
	{
		Far.AnyEditor.AfterOpen += OnAfterOpen;
	}

	public override void Disconnect()
	{
		Far.AnyEditor.AfterOpen -= OnAfterOpen;
	}

	void OnAfterOpen(object sender, EventArgs e)
	{
		IEditor editor = (IEditor)sender;
		if (editor.FileName.ToLower().EndsWith(".txt"))
			editor.BeforeSave += OnBeforeSave;
	}

	void OnBeforeSave(object sender, EventArgs e)
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
