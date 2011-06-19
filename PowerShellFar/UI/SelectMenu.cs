
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.UI
{
	static class SelectMenu
	{
		/// <summary>
		/// Returns e.g. MyDrive:
		/// </summary>
		internal static string SelectDrive(string select, bool extras)
		{
			IMenu m = Far.Net.CreateMenu();
			m.AutoAssignHotkeys = true;
			m.Title = "Power panel";
			m.HelpTopic = Far.Net.GetHelpTopic("MenuPanels");
			if (extras)
			{
				m.Add("Folder &tree");
				m.Add("&Any objects");
				m.Add(string.Empty).IsSeparator = true;
			}

			int i = extras ? 2 : -1;
			foreach (object o in A.InvokeCode("Get-PowerShellFarDriveName"))
			{
				++i;
				FarItem mi = m.Add(o.ToString());
				if (mi.Text.Length == 0)
				{
					mi.IsSeparator = true;
					continue;
				}
				if (mi.Text == select)
					m.Selected = i;
				mi.Text += ':';
			}

			if (!m.Show())
				return null;

			return m.Items[m.Selected].Text;
		}
		/// <summary>
		/// Select a share
		/// </summary>
		internal static string SelectShare(string computer)
		{
			const string code = @"
Get-WmiObject -Class Win32_Share -ComputerName $args[0] |
Sort-Object Name |
.{process{ $_.Name; $_.Description }}
";
			Collection<PSObject> values = A.InvokeCode(code, computer);

			IMenu m = Far.Net.CreateMenu();
			m.AutoAssignHotkeys = true;
			m.Title = computer + " shares";
			for (int i = 0; i < values.Count; i += 2)
			{
				string name = values[i].ToString();
				string desc = values[i + 1].ToString();
				if (desc.Length > 0)
					name += " (" + desc + ")";
				m.Add(name);
			}
			if (!m.Show())
				return null;

			return values[2 * m.Selected].ToString();
		}
	}
}
