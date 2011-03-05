
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
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
		internal ListPanel(Explorer explorer)
			: base(explorer)
		{
			PostName(_lastCurrentName);
			UseFilter = true;

			// 090411 Use custom Descriptions mode
			PanelPlan plan = new PanelPlan();
			plan.Columns = new FarColumn[]
			{
				new SetColumn() { Kind = "N", Name = "Name" },
				new SetColumn() { Kind = "Z", Name = "Value" }
			};
			SetPlan(PanelViewMode.AlternativeFull, plan);

			InvokingCommand += OnInvokingCommand;
		}
		/// <summary>
		/// The target object.
		/// </summary>
		internal abstract PSObject Target { get; }
		/// <summary>
		/// Puts a value into the command line or opens a lookup panel or member panel.
		/// </summary>
		public override void OpenFile(FarFile file)
		{
			if (file == null)
				throw new ArgumentNullException("file");

			PSPropertyInfo pi = file.Data as PSPropertyInfo;

			// e.g. visible mode: sender is MemberDefinition
			if (pi == null)
				return;

			// lookup opener?
			if (_LookupOpeners != null)
			{
				ScriptHandler<FileEventArgs> handler;
				if (_LookupOpeners.TryGetValue(file.Name, out handler))
				{
					handler.Invoke(this, new FileEventArgs(file));
					return;
				}
			}

			// case: can show value in the command line
			string s = Converter.InfoToLine(pi);
			if (s != null)
			{
				// set command line
				ILine cl = Far.Net.CommandLine;
				cl.Text = "=" + s;
				cl.SelectText(1, s.Length + 1);
				return;
			}

			// case: enumerable
			IEnumerable ie = Cast<IEnumerable>.From(pi.Value);
			if (ie != null)
			{
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(ie);
				op.OpenChild(this);
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
			MemberPanel r = new MemberPanel(new MemberExplorer(pi.Value));
			r.OpenChild(this);
			return r;
		}
		/// <summary>
		/// Sets new value.
		/// </summary>
		/// <param name="info">Property info.</param>
		/// <param name="value">New value.</param>
		internal abstract void SetUserValue(PSPropertyInfo info, string value);
		/// <summary>
		/// Calls base or assigns a value to the current property.
		/// </summary>
		void OnInvokingCommand(object sender, CommandLineEventArgs e)
		{
			// base
			string code = e.Command.TrimStart();
			if (!code.StartsWith("=", StringComparison.Ordinal))
				return;

			// we do
			e.Ignore = true;

			// skip empty
			FarFile f = CurrentFile;
			if (f == null)
				return;
			PSPropertyInfo pi = f.Data as PSPropertyInfo;
			if (pi == null)
				return;

			try
			{
				SetUserValue(pi, code.Substring(1));
				UpdateRedraw(true);
			}
			catch (RuntimeException ex)
			{
				A.Message(ex.Message);
			}
		}
		///?? Must be called last
		protected override bool CanClose()
		{
			if (Child != null)
				return true;

			FarFile f = CurrentFile;
			if (f == null)
				_lastCurrentName = null;
			else
				_lastCurrentName = f.Name;

			return true;
		}
		internal override void ShowHelp()
		{
			Help.ShowTopic("ListPanel");
		}
		internal override void UIApply()
		{
			A.InvokePipelineForEach(new PSObject[] { Target });
		}
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.ApplyCommand == null)
			{
				items.ApplyCommand = new SetItem()
				{
					Text = Res.UIApply,
					Click = delegate { UIApply(); }
				};
			}

			base.HelpMenuInitItems(items, e);
		}
		/// <summary>
		/// It deletes property values = assigns nulls.
		/// </summary>
		internal void UISetNulls()
		{
			foreach (FarFile file in SelectedFiles)
			{
				PSPropertyInfo pi = file.Data as PSPropertyInfo;
				if (pi == null)
					continue;

				try
				{
					SetUserValue(pi, null);
					UpdateRedraw(true);
				}
				catch (RuntimeException ex)
				{
					A.Message(ex.Message);
				}
			}
		}
		///
		public override bool UIKeyPressed(int code, KeyStates state)
		{
			switch (code)
			{
				case VKeyCode.Delete:
					
					goto case VKeyCode.F8;
				
				case VKeyCode.F8:
					
					if (state == KeyStates.Shift)
					{
						UISetNulls();
						return true;
					}
					
					break;
			}
			
			// base
			return base.UIKeyPressed(code, state);
		}
	}
}
