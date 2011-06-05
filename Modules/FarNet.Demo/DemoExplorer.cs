
using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
namespace FarNet.Demo
{
	/// <summary>
	/// Panel file explorer to view .resources file data.
	/// </summary>
	class DemoExplorer : Explorer
	{
		readonly string FileName;
		readonly List<FarFile> Files;
		public DemoExplorer(string fileName)
			: base(new Guid("83c6c606-e8fb-4fbb-87ab-e41e617589bd"))
		{
			FileName = fileName;
			Files = new List<FarFile>();
			
			// Read the .resources file, create and cache the panels files
			using (var reader = new ResourceReader(fileName))
			{
				var it = reader.GetEnumerator();
				while (it.MoveNext())
				{
					SetFile file = new SetFile();
					file.Name = it.Key.ToString();
					file.Description = Convert.ToString(it.Value);
					Files.Add(file);
				}
			}
		}
		public override IList<FarFile> GetFiles(GetFilesEventArgs args)
		{
			// Return the ready file list. It never changes and this
			// method does not have to create files again on requests.
			return Files;
		}
		public override Panel CreatePanel()
		{
			Panel panel = new Panel(this);
			panel.HostFile = FileName;
			panel.Title = Path.GetFileName(FileName);

			// Set sort and view modes
			panel.SortMode = PanelSortMode.Name;
			panel.ViewMode = PanelViewMode.AlternativeFull;

			// Define the panel columns
			PanelPlan plan = new PanelPlan();
			plan.Columns = new FarColumn[]
			{
				new SetColumn() { Kind = "N", Name = "Name" },
				new SetColumn() { Kind = "Z", Name = "Value" }
			};
			panel.SetPlan(PanelViewMode.AlternativeFull, plan);

			return panel;
		}
	}
}
