
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar.UI
{
	class ModulesMenu
	{
		IMenu _menu = Far.Net.CreateMenu();

		public ModulesMenu()
		{
			_menu.Title = "PowerShell Modules and Snap-ins";
			_menu.Bottom = "[Enter] import/remove, [Esc] exit";
			_menu.HelpTopic = A.Psf.HelpTopic + "MenuModules";
			_menu.AutoAssignHotkeys = true;
		}

		public void Show()
		{
			// available modules
			Collection<PSObject> modules1 = A.Psf.InvokeCode("Get-Module -ListAvailable");
			if (modules1.Count > 0)
			{
				Collection<PSObject> modules2 = A.Psf.InvokeCode("Get-Module");
				foreach (PSObject o1 in modules1)
				{
					PSModuleInfo info1 = (PSModuleInfo)o1.BaseObject;
					string memo = string.IsNullOrEmpty(info1.Description) ? info1.Path : info1.Description;
					FarItem item = _menu.Add(info1.Name + " module: " + memo);
					item.Data = info1;
					foreach (PSObject o2 in modules2)
					{
						PSModuleInfo info2 = (PSModuleInfo)o2.BaseObject;
						if (info2.Name == info1.Name)
						{
							item.Checked = true;
							break;
						}
					}
				}
			}

			// registered snapins
			Collection<PSObject> snapins1 = A.Psf.InvokeCode("Get-PSSnapin -Registered");
			if (snapins1.Count > 0)
			{
				Collection<PSObject> snapins2 = A.Psf.InvokeCode("Get-PSSnapin");
				foreach (PSObject o1 in snapins1)
				{
					PSSnapInInfo info1 = (PSSnapInInfo)o1.BaseObject;
					string memo = string.IsNullOrEmpty(info1.Description) ? info1.ModuleName : info1.Description;
					FarItem item = _menu.Add(info1.Name + " snap-in: " + memo);
					item.Data = info1;
					foreach (PSObject o2 in snapins2)
					{
						PSSnapInInfo info2 = (PSSnapInInfo)o2.BaseObject;
						if (info2.Name == info1.Name)
						{
							item.Checked = true;
							break;
						}
					}
				}
			}

			// loop
			while (_menu.Show())
			{
				FarItem item = _menu.Items[_menu.Selected];
				PSModuleInfo module = item.Data as PSModuleInfo;
				if (module != null)
				{
					// remove/import
					if (item.Checked)
						A.Psf.InvokeCode("Remove-Module -ModuleInfo $args[0]", module);
					else
						A.Psf.InvokeCode("Import-Module -ModuleInfo $args[0]", module);
				}
				else
				{
					// remove/add
					PSSnapInInfo snapin = (PSSnapInInfo)item.Data;
					PSSnapInException ex;
					if (item.Checked)
						A.Psf.Runspace.RunspaceConfiguration.RemovePSSnapIn(snapin.Name, out ex);
					else
						A.Psf.Runspace.RunspaceConfiguration.AddPSSnapIn(snapin.Name, out ex);

					// state is unknown on error, break the menu
					if (ex != null)
						throw ex;
				}
				item.Checked = !item.Checked;
			}
		}

	}
}
