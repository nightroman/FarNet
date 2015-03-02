
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Data;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Object member panel, e.g. property list to view or edit values.
	/// </summary>
	public sealed class MemberPanel : ListPanel
	{
		/// <summary>
		/// Gets the panel explorer.
		/// </summary>
		public new MemberExplorer Explorer { get { return (MemberExplorer)base.Explorer; } }
		/// <summary>
		/// New member panel with the member explorer.
		/// </summary>
		/// <param name="explorer">The panel explorer.</param>
		public MemberPanel(MemberExplorer explorer)
			: base(explorer)
		{
			// panel info
			CurrentLocation = "*";
			SortMode = PanelSortMode.Unsorted;
		}
		///
		protected override string DefaultTitle { get { return "Members: " + Target.BaseObject.GetType().Name; } }
		internal sealed override PSObject Target
		{
			get { return Explorer.Value; }
		}
		/// <summary>
		/// Gets or sets data modification flag.
		/// </summary>
		/// <remarks>
		/// It is set internally on any interactive data change.
		/// If data are changed externally the flag should be set.
		/// If this flag is set the panel asks to save modified data
		/// and calls the <see cref="AsSaveData"/> script, if any.
		/// </remarks>
		public bool Modified { get; set; }
		/// <summary>
		/// Object which member list is shown at the panel.
		/// </summary>
		public PSObject Value
		{
			get { return Target; }
		}
		// Propagates exclude and hidden member patterns from the parent table.
		void UpdateExplorerPatterns()
		{
			TablePanel table = Parent as TablePanel;
			if (table == null)
				return;

			if (string.IsNullOrEmpty(Explorer.ExcludeMemberPattern) && !string.IsNullOrEmpty(table.ExcludeMemberPattern))
				Explorer.ExcludeMemberPattern = table.ExcludeMemberPattern;

			if (string.IsNullOrEmpty(Explorer.HideMemberPattern) && !string.IsNullOrEmpty(table.HideMemberPattern))
				Explorer.HideMemberPattern = table.HideMemberPattern;
		}
		internal override MemberPanel OpenFileMembers(FarFile file)
		{
			PSObject o = PSObject.AsPSObject(file.Data);
			string memberType = o.Properties["MemberType"].Value.ToString();
			if (!memberType.EndsWith("Property", StringComparison.Ordinal)) //??
				return null;

			string name = o.Properties[Word.Name].Value.ToString();
			object instance = Target.Properties[name].Value;
			if (instance == null)
				return null;

			MemberPanel r = new MemberPanel(new MemberExplorer(instance));
			r.OpenChild(this);
			return r;
		}
		/// <summary>
		/// Should be called when an item property is changed.
		/// </summary>
		internal static void WhenMemberChanged(object instance)
		{
			foreach (MemberPanel p in Far.Api.Panels(typeof(MemberPanel)))
			{
				if (p.Target == instance)
				{
					p.Modified = true;
					p.UpdateRedraw(true);
				}
			}
		}
		/// <summary>
		/// Changes modes: properties, members, static
		/// </summary>
		internal override void UIMode()
		{
			if (++Explorer.MemberMode > 1) // don't use static for now
				Explorer.MemberMode = 0;
			UpdateRedraw(false, 0, 0);
		}
		/// <summary>
		/// Creates a lookup handler designed only for <see cref="DataRow"/>.
		/// </summary>
		/// <param name="namePairs">Destination and source field name pairs.</param>
		/// <returns>Lookup handler to assigned to <see cref="AnyPanel.Lookup"/>.</returns>
		/// <remarks>
		/// This panel <see cref="Value"/> and lookup panel items should be <see cref="DataRow"/> objects,
		/// e.g. this panel shows members of a row from parent <see cref="DataPanel"/>
		/// and a lookup panel is also <see cref="DataPanel"/>.
		/// <para>
		/// The returned handler copies data from the source (lookup) row to the destination row using
		/// destination and source field name pairs, e.g.: <c>dst1, src1 [, dst2, src2 [, ...]]</c>.
		/// Example script: <c>Test-Panel-DbNotes-.ps1</c>.
		/// </para>
		/// </remarks>
		public EventHandler<OpenFileEventArgs> CreateDataLookup(string[] namePairs)
		{
			if (Cast<DataRow>.From(Target) == null)
				throw new InvalidOperationException("Data lookup is designed only for data row objects.");

			if (namePairs == null || namePairs.Length == 0)
				throw new ArgumentException("'namePairs' must not be null or empty.");

			if (namePairs.Length % 2 != 0)
				throw new ArgumentException("'namePairs' must contain even number of items.");

			return (new DataLookup(namePairs)).Invoke;
		}
		/// <summary>
		/// Calls one of:
		/// *) the <see cref="AsSaveData"/> script, if any;
		/// *) the parent panel to save data if this is a child panel.
		/// </summary>
		public override bool SaveData()
		{
			if (AsSaveData != null)
			{
				A.InvokeScriptReturnAsIs(AsSaveData, this);
				return !Modified;
			}

			if (Parent != null)
				return Parent.SaveData();

			return true;
		}
		internal override void SetUserValue(PSPropertyInfo info, string value)
		{
			try
			{
				// assign
				if (value == null)
					info.Value = null;
				else
					//! it is tempting to avoid our parsing, but it is not that good..
					info.Value = Converter.Parse(info, value);

				// change is done
				WhenMemberChanged(Target);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}
		/// <summary>
		/// Gets or sets the script called to save modified data.
		/// It has to save data and set <see cref="Modified"/> to false.
		/// </summary>
		public ScriptBlock AsSaveData { get; set; }
		///??
		protected override bool CanClose()
		{
			// can?
			bool r = !Modified || AsSaveData == null;

			// ask
			if (!r)
			{
				switch (Far.Api.Message(Res.AskSaveModified, "Save", MessageOptions.YesNoCancel))
				{
					case 0:
						A.InvokeScriptReturnAsIs(AsSaveData, this);
						break;
					case 1:
						Modified = false;
						break;
				}
				r = !Modified;
			}

			if (!r)
				return false;

			return base.CanClose();
		}
		///
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.Save == null && (AsSaveData != null || Parent != null && (Parent is DataPanel)))
				items.Save = new SetItem()
				{
					Text = "Save data",
					Click = delegate { SaveData(); }
				};

			base.HelpMenuInitItems(items, e);
		}
		/// <inheritdoc/>
		public override void UICreateFile(CreateFileEventArgs args)
		{
			if (args == null) return;

			// skip data panel
			if (Parent is DataPanel)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// call
			Explorer.CreateFile(args);
			if (args.Result != JobResult.Done)
				return;

			// update that panel if the instance is the same
			MemberPanel that = TargetPanel as MemberPanel;
			if (that != null && that.Target == Target)
				that.UpdateRedraw(true);
		}
	}
}
