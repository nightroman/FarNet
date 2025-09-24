using FarNet;
using static PowerShellFar.Interactive;

namespace PowerShellFar.UI;

static class SessionsMenu
{
	const string
		TextTitle = "Sessions",
		TextBottom = "Enter, Del",
		TextNewSession = "New session";

	public static Session? Select(List<Session> sessions)
	{
		var menu = Far.Api.CreateMenu();
		menu.Title = TextTitle;
		menu.Bottom = TextBottom;

		menu.AddKey(KeyCode.Delete);

		for (; ; menu.Items.Clear())
		{
			foreach (var ses in sessions)
				menu.Add($"{ses.Runspace.Id} {ses.Runspace.Name}").Data = ses;

			menu.Add(TextNewSession);

			if (!menu.Show())
				continue;

			var selected = menu.SelectedData as Session;

			// Del
			if (menu.Key.Is(KeyCode.Delete))
			{
				if (selected is { })
				{
					selected.Runspace.Close();
					sessions.Remove(selected);
				}
				continue;
			}

			// Enter
			return selected;
		}
	}
}
