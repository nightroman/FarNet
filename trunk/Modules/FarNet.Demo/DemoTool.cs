
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
namespace FarNet.Demo
{
	/// <summary>
	/// Provides a menu item in all plugin menus.
	/// It sets the level of tracing used by the core and turn tracing to the file on/off.
	/// The file trace listener is added to the <see cref="Log.Source"/> (FarNet) and <see cref="Trace"/> (.NET).
	/// </summary>
	/// <remarks>
	/// The plugin menu item is shown in English or Russian, according to the current UI settings.
	/// The attribute property <c>Name</c> is treated as a resource (due to the <c>Resources</c> flag).
	/// </remarks>
	[ModuleTool(Name = "MenuTitle", Options = ModuleToolOptions.AllAreas, Resources = true)]
	[Guid("a10218a8-76b3-47f7-8900-3a162bf16c49")]
	public class DemoTool : ModuleTool
	{
		const string TracingFile = "FarNet.Demo.tracing";
		static TextWriterTraceListener _Listener;
		/// <summary>
		/// This method implements the menu tool action.
		/// </summary>
		public override void Invoke(object sender, ModuleToolEventArgs e)
		{
			// tracing file is in the local module data directory
			var fileName = Path.Combine(Manager.GetFolderPath(SpecialFolder.LocalData, true), TracingFile);

			// create the menu instance
			var menu = Far.Net.CreateMenu();
			menu.Title = GetString("MenuTitle");

			// set this menu help topic
			menu.HelpTopic = Far.Net.GetHelpTopic("TracingMenu");

			// add the menu items

			var itemShowFile = menu.Add(GetString("itemShowFile"));
			var itemListener = menu.Add(GetString("itemListener"));

			menu.Add(string.Empty).IsSeparator = true;

			var itemWarning = menu.Add(GetString("itemWarning"));
			itemWarning.Data = SourceLevels.Warning;

			var itemInformation = menu.Add(GetString("itemInformation"));
			itemInformation.Data = SourceLevels.Information;

			var itemVerbose = menu.Add(GetString("itemVerbose"));
			itemVerbose.Data = SourceLevels.Verbose;

			var itemAll = menu.Add(GetString("itemAll"));
			itemAll.Data = SourceLevels.All;

			// show the menu repeatedly until it is canceled
			for (; ; )
			{
				// update disabled and checked states
				itemShowFile.Disabled = !File.Exists(fileName);
				itemListener.Checked = _Listener != null;
				itemWarning.Checked = Log.Source.Switch.Level == SourceLevels.Warning;
				itemInformation.Checked = Log.Source.Switch.Level == SourceLevels.Information;
				itemVerbose.Checked = Log.Source.Switch.Level == SourceLevels.Verbose;
				itemAll.Checked = Log.Source.Switch.Level == SourceLevels.All;

				// show the menu, exit on cancel
				if (!menu.Show())
					return;

				// use the selected data to change tracing level, then continue
				if (menu.SelectedData != null)
				{
					Log.Source.Switch.Level = (SourceLevels)menu.SelectedData;
					continue;
				}

				// use the selected index for other commands
				switch (menu.Selected)
				{
					case 0:
						// open the tracing file in the viewer
						var viewer = Far.Net.CreateViewer();
						viewer.Switching = Switching.Disabled;
						viewer.FileName = fileName;
						viewer.Open();
						return;
					case 1:
						// turn tracing to file on/off
						if (_Listener == null)
						{
							_Listener = new TextWriterTraceListener(fileName, TracingFile);
							Log.Source.Listeners.Add(_Listener);
							Trace.Listeners.Add(_Listener);
						}
						else
						{
							Log.Source.Listeners.Remove(_Listener);
							Trace.Listeners.Remove(_Listener);
							_Listener.Close();
							_Listener = null;
						}
						continue;
				}
			}
		}
	}
}
