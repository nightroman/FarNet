
using FarNet;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

// Test filer for *.filer files with "FILER" header
[ModuleFiler(Name = TestFiler.Name, Mask = "*.filer")]
[Guid("7465ee92-dbef-4757-a92c-7ebc82abac67")]
public class TestFiler : ModuleFiler
{
	public const string Name = "TestFiler";

	// Shows data in a message box and lines in a panel
	public override void Invoke(object sender, ModuleFilerEventArgs e)
	{
		// read and check the header from sent data
		byte[] buffer = new byte[5];
		int read = e.Data.Read(buffer, 0, 5);
		string header = Encoding.Default.GetString(buffer, 0, read);
		Far.Net.Message(header, "File header");
		if (header != "FILER")
			return;

		// create panel
		IPanel p = Far.Net.CreatePanel();
		p.Info.HostFile = e.Name;
		p.Info.StartSortMode = PanelSortMode.Unsorted;
		p.Info.Title = "File lines";

		// read lines
		foreach (string s in File.ReadAllLines(e.Name))
		{
			SetFile f = new SetFile();
			f.Name = s;
			p.Files.Add(f);
		}
		p.Open();
	}
}
