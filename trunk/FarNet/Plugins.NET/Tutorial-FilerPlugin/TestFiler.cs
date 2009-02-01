using System;
using System.IO;
using System.Text;
using FarNet;

/// Test filer for *.test files with "TEST" header
public class TestFiler : FilerPlugin
{
	/// Shows data in a message box and lines in a panel
	public override void Invoke(object sender, FilerEventArgs e)
	{
		// read and check the header from sent data
		byte[] buffer = new byte[4];
		int read = e.Data.Read(buffer, 0, 4);
		string header = Encoding.Default.GetString(buffer, 0, read);
		Far.Msg(header, "File header");
		if (header != "TEST")
			return;

		// create panel
		IPluginPanel p = Far.CreatePluginPanel();
		p.Info.HostFile = e.Name;
		p.Info.StartSortMode = PanelSortMode.Unsorted;
		p.Info.Title = "File lines";

		// read lines
		foreach (string s in File.ReadAllLines(e.Name))
		{
			IFile f = Far.CreatePanelItem();
			f.Name = s;
			p.Files.Add(f);
		}
		p.Open();
	}

	/// Default mask
	public override string Mask
	{
		get { return "*.test"; }
	}
}
