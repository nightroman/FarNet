using System.Globalization;

namespace FarNet.Works;

static class ConfigUICulture
{
	const string HelpTopic = "module-ui-culture";

	public static void Show(IModuleManager? manager)
	{
		var menu = Far.Api.CreateMenu();
		menu.AutoAssignHotkeys = true;
		menu.HelpTopic = HelpTopic;
		menu.Title = "Module UI culture";

		List<IModuleManager> managers = new(manager is null ? ModuleLoader.GetModuleManagers() : [manager]);
		int max1 = managers.Count > 0 ? managers.Max(x => x.ModuleName.Length) : 0;
		for (; ; )
		{
			menu.Items.Clear();
			foreach (IModuleManager it in managers)
				menu.Add($"{it.ModuleName.PadRight(max1)} : {it.StoredUICulture}").Data = it;

			if (!menu.Show())
				return;

			manager = (IModuleManager)menu.SelectedData!;

			// show the input box
			var ib = Far.Api.CreateInputBox();
			ib.Title = manager.ModuleName;
			ib.Prompt = "Culture name (empty = the Far culture)";
			ib.Text = manager.StoredUICulture;
			ib.History = "Culture";
			ib.HelpTopic = menu.HelpTopic;
			ib.EmptyEnabled = true;
			if (!ib.Show())
				continue;

			// set the culture, even the same, to refresh
			var cultureName = ib.Text;
			CultureInfo ci;
			try
			{
				// get the culture by name, it may throw
				ci = CultureInfo.GetCultureInfo(cultureName);

				// save the name from the culture, not from a user
				manager.StoredUICulture = ci.Name;
				manager.SaveConfig();

				// use the current Far culture instead of invariant
				if (ci.Name.Length == 0)
					ci = Far.Api.GetCurrentUICulture(true);

				// update the module
				manager.CurrentUICulture = ci;
			}
			catch (ArgumentException)
			{
				Far.Api.Message("Unknown culture name.");
			}
		}
	}
}
