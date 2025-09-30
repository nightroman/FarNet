using FarNet;

namespace PowerShellFar.UI;

static class SelectMenu
{
	internal const string TextFolderTree = "Folder &tree";
	internal const string TextAnyObjects = "&Any objects";

	/// <summary>
	/// Returns the special item or drive text ("HKLM:").
	/// </summary>
	internal static string? SelectPowerPanel()
	{
		var m = Far.Api.CreateMenu();
		m.AutoAssignHotkeys = true;
		m.Title = "Power panel";
		m.HelpTopic = Entry.Instance.GetHelpTopic(HelpTopic.PowerPanelMenu);

		m.Add(TextFolderTree);
		m.Add(TextAnyObjects);
		m.Add(string.Empty).IsSeparator = true;

		var drives = A.Runspace.SessionStateProxy.Drive.GetAll();
		foreach (var drive in drives)
		{
			m.Add(drive.Name + ':');
		}

		if (!m.Show())
			return null;

		return m.Items[m.Selected].Text;
	}
}
