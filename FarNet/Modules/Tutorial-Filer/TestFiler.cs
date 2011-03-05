
using FarNet;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

// Test filer for *.filer files with "FILER" header
[System.Runtime.InteropServices.Guid("7465ee92-dbef-4757-a92c-7ebc82abac67")]
[ModuleFiler(Name = TestFiler.Name, Mask = "*.filer")]
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

		(new TheExplorer(e.Name)).OpenPanel();
	}
}

// Panel explorer
class TheExplorer : Explorer
{
	string FileName;
	public TheExplorer(string fileName)
		: base(new Guid("83c6c606-e8fb-4fbb-87ab-e41e617589bd"))
	{
		FileName = fileName;
	}
	public override IList<FarFile> GetFiles(GetFilesEventArgs args)
	{
		List<FarFile> files = new List<FarFile>();
		foreach (string line in File.ReadAllLines(FileName))
		{
			SetFile file = new SetFile();
			file.Name = line;
			files.Add(file);
		}
		return files;
	}
	public override Panel CreatePanel()
	{
		Panel panel = new Panel(this);
		panel.HostFile = FileName;
		panel.Title = "File lines";
		panel.SortMode = PanelSortMode.Unsorted;
		return panel;
	}
}
