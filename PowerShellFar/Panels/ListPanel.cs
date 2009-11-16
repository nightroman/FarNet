/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Base panel for property list, member list and etc.
	/// </summary>
	public abstract class ListPanel : AnyPanel
	{
		static string _lastCurrentName;

		internal ListPanel()
		{
			Panel.PostName(_lastCurrentName);
			Panel.Info.UseAttrHighlighting = true;
			Panel.Info.UseFilter = true;
			Panel.Info.UseHighlighting = true;

			// 090411 Use custom Descriptions mode
			PanelModeInfo mode = new PanelModeInfo();
			SetColumn c1 = new SetColumn(); c1.Type = "N"; c1.Name = "Name";
			SetColumn c2 = new SetColumn(); c2.Type = "Z"; c2.Name = "Value";
			mode.Columns = new FarColumn[] { c1, c2 };
			Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode);
		}

		/// <summary>
		/// Puts a value into the command line or opens a lookup panel or member panel.
		/// </summary>
		public override void OpenFile(FarFile file)
		{
			PSPropertyInfo pi = file.Data as PSPropertyInfo;

			// e.g. visible mode: sender is MemberDefinition
			if (pi == null)
				return;

			// lookup opener?
			if (_LookupOpeners != null)
			{
				EventHandler<FileEventArgs> handler;
				if (_LookupOpeners.TryGetValue(file.Name, out handler))
				{
					handler(this, new FileEventArgs(file));
					return;
				}
			}

			// case: can show value in the command line
			string s = Converter.InfoToLine(pi);
			if (s != null)
			{
				// set command line
				ILine cl = A.Far.CommandLine;
				cl.Text = "=" + s;
				cl.Select(1, s.Length + 1);
				return;
			}

			// case: enumerable
			IEnumerable ie = Convert<IEnumerable>.From(pi.Value);
			if (ie != null)
			{
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(ie);
				op.ShowAsChild(this);
				return;
			}

			// open members
			OpenFileMembers(file);
		}

		internal override MemberPanel OpenFileMembers(FarFile file)
		{
			PSPropertyInfo pi = file.Data as PSPropertyInfo;
			if (pi == null)
				return null;
			if (pi.Value == null)
				return null;
			MemberPanel r = new MemberPanel(pi.Value);
			r.ShowAsChild(this);
			return r;
		}

		/// <summary>
		/// Sets new value.
		/// </summary>
		/// <param name="info">Property info.</param>
		/// <param name="value">New value.</param>
		internal abstract void SetUserValue(PSPropertyInfo info, string value);

		internal override bool UICommand(string code)
		{
			// base
			code = code.TrimStart();
			if (!code.StartsWith("=", StringComparison.Ordinal))
				return base.UICommand(code);

			// skip empty
			FarFile f = Panel.CurrentFile;
			if (f == null)
				return true;
			PSPropertyInfo pi = f.Data as PSPropertyInfo;
			if (pi == null)
				return true;

			try
			{
				SetUserValue(pi, code.Substring(1));
				UpdateRedraw(true);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex.Message);
			}

			return true;
		}

		// Must be called last
		internal override bool CanClose()
		{
			if (Child != null)
				return true;

			FarFile f = Panel.CurrentFile;
			if (f == null)
				_lastCurrentName = null;
			else
				_lastCurrentName = f.Name;

			return true;
		}

		internal override void ShowHelp()
		{
			A.Far.ShowHelp(A.Psf.AppHome, "ListPanel", HelpOptions.Path);
		}

	}
}
