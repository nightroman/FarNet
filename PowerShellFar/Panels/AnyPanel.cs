
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
	/// Base class of PowerShellFar panels.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Terminology (both for names and documentation):
	/// "files" (<c>FarFile</c>) are elements of <see cref="Panel"/>;
	/// "items" (<c>PSObject</c>) are <see cref="FarFile.Data"/> attached to "files".
	/// Note that null and ".." items are not processed (e.g. by <see cref="ShownItems"/>
	/// and <see cref="SelectedItems"/>).
	/// </para>
	/// </remarks>
	public abstract partial class AnyPanel : Panel
	{
		/// <summary>
		/// Default panel.
		/// </summary>
		internal AnyPanel(Explorer explorer)
			: base(explorer)
		{
			// settings
			DotsMode = PanelDotsMode.Dots;

			// start mode
			ViewMode = PanelViewMode.AlternativeFull;
		}
		static PSObject ConvertFileToItem(FarFile file)
		{
			if (file == null || file.Data == null)
				return null;

			return PSObject.AsPSObject(file.Data);
		}
		/// <summary>
		/// Gets all items. See <see cref="AnyPanel"/> remarks.
		/// </summary>
		/// <remarks>
		/// Items are returned according to the current panel filter and sort order.
		/// <para>
		/// WARNING: it is <c>IEnumerable</c>, not a list or an array.
		/// </para>
		/// <example>
		/// OK:
		/// <code>
		/// foreach($item in $panel.ShownItems) { ... }
		/// $panel.ShownItems | ...
		/// </code>
		/// ERROR:
		/// <code>
		/// $panel.ShownItems.Count
		/// $panel.ShownItems[$index]
		/// </code>
		/// OK: use operator @() when you really want an array:
		/// <code>
		/// $items = @($panel.ShownItems)
		/// $items.Count
		/// $items[$index]
		/// </code>
		/// </example>
		/// </remarks>
		public IEnumerable<PSObject> ShownItems
		{
			get
			{
				foreach (FarFile file in ShownFiles)
					yield return ConvertFileToItem(file);
			}
		}
		/// <summary>
		/// Gets selected items. See <see cref="AnyPanel"/> and <see cref="ShownItems"/> remarks.
		/// </summary>
		public IEnumerable<PSObject> SelectedItems
		{
			get
			{
				foreach (FarFile file in SelectedFiles)
					yield return ConvertFileToItem(file);
			}
		}
		/// <summary>
		/// Gets the current item. It can be null (e.g. if ".." is the current file).
		/// </summary>
		public PSObject CurrentItem
		{
			get
			{
				return ConvertFileToItem(CurrentFile);
			}
		}
		/// <summary>
		/// Tells to treat the items as not directories even if they have a directory flag.
		/// </summary>
		internal bool IgnoreDirectoryFlag { get; set; } // _090810_180151
		///
		protected override bool OpenChildBegin(Panel parent)
		{
			if (parent == null) throw new ArgumentNullException("parent");

			if (Lookup == null)
				return true;

			// lookup: try to post the current
			// 090809 ??? perhaps I have to rethink
			TablePanel tp = this as TablePanel;
			if (tp != null)
			{
				// assume parent Description = child Name
				string value = parent.CurrentFile.Description;
				PostName(value);
			}

			return true;
		}
		/// <summary>
		/// Shows help.
		/// </summary>
		internal virtual void ShowHelp()
		{
			Help.ShowTopic("PowerPanel");
		}
		/// <summary>
		/// Shows help menu (e.g. called on [F1]).
		/// </summary>
		/// <seealso cref="MenuCreating"/>
		public void ShowMenu()
		{
			IMenu menu = HelpMenuCreate();
			if (menu.Show())
			{
				if (menu.BreakKey == VKeyCode.F1)
					ShowHelp();
			}
		}
		/// <summary>Apply command.</summary>
		internal virtual void UIApply()
		{ }
		/// <summary>Attributes action.</summary>
		internal virtual void UIAttributes()
		{ }
		/// <summary>Copy here action.</summary>
		internal virtual void UICopyHere()
		{ }
		internal virtual bool UICopyMoveCan(bool move)
		{
			return !move && TargetPanel is ObjectPanel;
		}
		/// <summary>
		/// Shows help or the panel menu.
		/// </summary>
		internal void UIHelp()
		{
			ShowMenu();
		}
		/// <summary>Mode action.</summary>
		internal virtual void UIMode()
		{ }
		///
		internal void UIOpenFileMembers()
		{
			FarFile f = CurrentFile;
			if (f != null)
				OpenFileMembers(f);
		}
		/// <summary>Rename action.</summary>
		internal virtual void UIRename()
		{ }
		/// <summary>
		/// Updates Far data and redraws.
		/// </summary>
		internal void UpdateRedraw(bool keepSelection)
		{
			Update(keepSelection);
			Redraw();
		}
		/// <summary>
		/// Updates Far data and redraws with positions.
		/// </summary>
		internal void UpdateRedraw(bool keepSelection, int current, int top)
		{
			Update(keepSelection);
			Redraw(current, top);
		}
		/// <summary>
		/// Updates Far data and redraws.
		/// </summary>
		internal void UpdateRedraw(bool keepSelection, string setName)
		{
			Update(keepSelection);
			PostName(setName);
			Redraw();
		}
		/// <summary>
		/// Sets a handler called on [Enter] and makes this panel lookup.
		/// </summary>
		/// <remarks>
		/// If it is set than [Enter] triggers this handler and closes the panel.
		/// This is normally used for selecting an item from a table.
		/// The task of this handler is to use this selected item.
		/// <para>
		/// Lookup panels are usually derived from <see cref="TablePanel"/>.
		/// Panels derived from <see cref="ListPanel"/> also can be lookup but this scenario is not tested.
		/// </para>
		/// </remarks>
		/// <seealso cref="MemberPanel.CreateDataLookup"/>
		public ScriptHandler<FileEventArgs> Lookup { get; set; }
		/// <summary>
		/// Opens the file member panel.
		/// </summary>
		internal virtual MemberPanel OpenFileMembers(FarFile file)
		{
			//??? _090610_071700, + $panel.SetOpen({ @ Test-Panel-Tree-.ps1
			object target = file.Data == null ? file : file.Data;

			MemberPanel panel = new MemberPanel(new MemberExplorer(target));
			panel._LookupOpeners = _LookupOpeners;

			//! use null as parent: this panel can be not open now
			panel.OpenChild(null);
			return panel;
		}
		UserAction _UserWants;
		/// <summary>
		/// The last user action.
		/// </summary>
		internal UserAction UserWants
		{
			get { return _UserWants; }
		}
		/// <include file='doc.xml' path='doc/AddLookup/*'/>
		public void AddLookup(string name, object handler)
		{
			if (_LookupOpeners == null)
				_LookupOpeners = new Dictionary<string, ScriptHandler<FileEventArgs>>();

			_LookupOpeners.Add(name, new ScriptHandler<FileEventArgs>(handler));
		}
		/// <summary>
		/// Adds name/handler pairs to the lookup collection.
		/// </summary>
		public void AddLookup(IDictionary lookups)
		{
			if (lookups != null)
				foreach (DictionaryEntry it in lookups)
					AddLookup(it.Key.ToString(), it.Value);
		}
		internal Dictionary<string, ScriptHandler<FileEventArgs>> _LookupOpeners;
		/// <summary>
		/// Creates or gets existing menu.
		/// </summary>
		IMenu HelpMenuCreate()
		{
			// create
			IMenu r = Far.Net.CreateMenu();
			r.AutoAssignHotkeys = true;
			r.Sender = this;
			r.Title = "Help menu";
			r.BreakKeys.Add(VKeyCode.F1);

			// args
			PanelMenuEventArgs e = new PanelMenuEventArgs(r, CurrentFile, SelectedList);

			// event
			if (MenuCreating != null)
				MenuCreating(this, e);

			// init items
			HelpMenuItems items = new HelpMenuItems();
			HelpMenuInitItems(items, e);

			// add main items
			if (items.OpenFile != null) r.Items.Add(items.OpenFile);
			if (items.OpenFileMembers != null) r.Items.Add(items.OpenFileMembers);
			if (items.OpenFileAttributes != null) r.Items.Add(items.OpenFileAttributes);
			if (items.ApplyCommand != null) r.Items.Add(items.ApplyCommand);
			if (items.Copy != null) r.Items.Add(items.Copy);
			if (items.CopyHere != null) r.Items.Add(items.CopyHere);
			if (items.Move != null) r.Items.Add(items.Move);
			if (items.Rename != null) r.Items.Add(items.Rename);
			if (items.Create != null) r.Items.Add(items.Create);
			if (items.Delete != null) r.Items.Add(items.Delete);
			if (items.Save != null) r.Items.Add(items.Save);
			if (items.Exit != null) r.Items.Add(items.Exit);
			if (items.Help != null) r.Items.Add(items.Help);

			return r;
		}
		/// <summary>
		/// Derived should add its items, then call base.
		/// </summary>
		internal virtual void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.Exit == null)
				items.Exit = new SetItem()
				{
					Text = "E&xit panel",
					Click = delegate { Close(); }
				};

			if (items.Help == null)
				items.Help = new SetItem()
				{
					Text = "Help (F1)",
					Click = delegate { ShowHelp(); }
				};
		}
		/// <summary>
		/// Called when the panel help menu is just created (e.g. on [F1]).
		/// </summary>
		/// <remarks>
		/// You may add your menu items to the menu. The sender of the event is this panel (<c>$this</c>).
		/// </remarks>
		/// <seealso cref="ShowMenu"/>
		public event EventHandler<PanelMenuEventArgs> MenuCreating;
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override bool UIKeyPressed(int code, KeyStates state)
		{
			_UserWants = UserAction.None;
			try
			{
				switch (code)
				{
					case VKeyCode.Enter:

						switch (state)
						{
							case KeyStates.None:
								FarFile file = CurrentFile;
								if (file == null)
									break;

								_UserWants = UserAction.Enter;

								if (file.IsDirectory && !IgnoreDirectoryFlag)
									break;

								UIOpenFile(file);
								return true;

							case KeyStates.Shift:
								UIAttributes();
								return true;
						}

						break;

					case VKeyCode.F1:

						if (state == KeyStates.None)
						{
							UIHelp();
							return true;
						}

						break;

					case VKeyCode.F3:

						switch (state)
						{
							case KeyStates.None:
								if (CurrentFile == null)
								{
									UIViewAll();
									return true;
								}
								break;

							case KeyStates.Shift:
								ShowMenu();
								return true;
						}

						break;

					case VKeyCode.F5:

						switch (state)
						{
							case KeyStates.Shift:
								UICopyHere();
								return true;
						}

						break;

					case VKeyCode.F6:

						switch (state)
						{
							case KeyStates.Shift:
								UIRename();
								return true;
						}

						break;

					case VKeyCode.PageDown:

						if (state == KeyStates.Control)
						{
							UIOpenFileMembers();
							return true;
						}

						break;

					case VKeyCode.A:

						if (state == KeyStates.Control)
						{
							UIAttributes();
							return true;
						}

						break;

					case VKeyCode.G:

						if (state == KeyStates.Control)
						{
							UIApply();
							return true;
						}

						break;

					case VKeyCode.M:

						if (state == (KeyStates.Control | KeyStates.Shift))
						{
							UIMode();
							return true;
						}

						break;

					case VKeyCode.R:

						if (state == KeyStates.Control)
							_UserWants = UserAction.CtrlR;

						break;

					case VKeyCode.S:

						if (state == KeyStates.Control)
						{
							SaveData();
							return true;
						}

						break;
				}

				// base
				return base.UIKeyPressed(code, state);
			}
			finally
			{
				if (_UserWants != UserAction.CtrlR)
					_UserWants = UserAction.None;
			}
		}
	}
}
