
using FarNet;
using System;
using System.Collections.Generic;

namespace TryPanelCSharp
{
	// Demo explorer which creates and deletes virtual files.
	public class MyExplorer : Explorer
	{
		readonly List<FarFile> _files = new List<FarFile>();
		public MyExplorer()
			: base(new Guid("4bbffcb1-3570-4a95-8692-ccdd0c234a95"))
		{
			CanCreateFile = true;
			CanDeleteFiles = true;
			_files.Add(new SetFile() { Name = "Add [F7]; Remove [Del]/[F8]", Description = "demo file" });
		}
		public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
		{
			return _files;
		}
		public override void CreateFile(CreateFileEventArgs args)
		{
			var name = (string)args.Data;
			_files.Add(new SetFile() { Name = name, Description = "demo file" });
			args.PostName = name;
		}
		public override void DeleteFiles(DeleteFilesEventArgs args)
		{
			foreach (var file in args.Files)
				_files.RemoveAll((x) => x.Name == file.Name);
		}
	}

	// Demo panel with some user interaction.
	public class MyPanel : Panel
	{
		public MyPanel(Explorer explorer)
			: base(explorer)
		{
			SortMode = PanelSortMode.FullName;
			ViewMode = PanelViewMode.Descriptions;
		}
		public override void UICreateFile(CreateFileEventArgs args)
		{
			var name = Far.Api.Input("File name", null, "MyPanel");
			if (!String.IsNullOrEmpty(name))
			{
				args.Data = name;
				base.UICreateFile(args);
			}
		}
		public override void UIDeleteFiles(DeleteFilesEventArgs args)
		{
			if (0 == Far.Api.Message("Delete files?", "MyPanel", MessageOptions.OkCancel))
				base.UIDeleteFiles(args);
		}
	}

	// Demo tool with an item in the plugin menu.
	[System.Runtime.InteropServices.Guid("8f7ba3b1-7fe9-4912-9f30-b2f43ef47000")]
	[ModuleTool(Name = "TryPanelCSharp", Options = ModuleToolOptions.F11Menus)]
	public class MyTool : ModuleTool
	{
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			(new MyPanel(new MyExplorer())).Open();
		}
	}
}
