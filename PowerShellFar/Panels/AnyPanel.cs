
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		internal AnyPanel()
		{
			// settings
			DotsMode = PanelDotsMode.Dots;

			// add event handlers
			UpdateFiles += OnUpdateFiles;
			ExportFiles += OnExportFiles;
			KeyPressed += OnKeyPressed;
			SetPanelDirectory += OnSetDirectory;

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
		/// Thus, this set set is not the same as <see cref="Panel.Files"/>.
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
			
			if (_LookupCloser == null)
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

		/// <summary>
		/// Far handler.
		/// </summary>
		internal abstract void OnUpdateFiles(PanelEventArgs e);
		void OnUpdateFiles(object sender, PanelEventArgs e)
		{
			OnUpdateFiles(e);
		}

		/// <summary>
		/// Far handler. Actually used only for quick view of a file.
		/// </summary>
		void OnExportFiles(object sender, ExportFilesEventArgs e)
		{
			if ((e.Mode & ExplorerModes.QuickView) == 0 || e.Files.Count != 1)
				return;

			FarFile file = e.Files[0];
			if (file.Data == null)
				return;

			//! file.Alias must be null as we auto them
			WriteFile(file, My.PathEx.Combine(e.Destination, e.Names[0]));
		}

		/// <summary>Apply command.</summary>
		internal virtual void UIApply()
		{ }

		/// <summary>Attributes action.</summary>
		internal virtual void UIAttributes()
		{ }

		/// <summary>Create action.</summary>
		internal virtual void UICreate()
		{ }

		/// <summary>Copy here action.</summary>
		internal virtual void UICopyHere()
		{ }

		internal virtual bool UICopyMoveCan(bool move)
		{
			return !move && AnotherPanel is ObjectPanel;
		}

		/// <summary>
		/// Copy\move action.
		/// </summary>
		/// <returns>True if the action is done.</returns>
		/// <remarks>
		/// <see cref="AnyPanel"/> simply copies items to another <see cref="ObjectPanel"/>, if any.
		/// </remarks>
		internal virtual bool UICopyMove(bool move)
		{
			if (move)
				return false;

			ObjectPanel that = AnotherPanel as ObjectPanel;
			if (that == null)
				return false;

			// _100227_073909
			// add objects to the target object panel and update it
			//! we allow dupes on the target panel, why not? a user is the boss
			//! keep the selection, the temp panel keeps it; note: selection is by names, added objects may get selected
			that.AddObjects(SelectedItems);
			that.UpdateRedraw(true);

			return true;
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

		// Event handler is called before the Panel processing, so do not care about the base
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void OnKeyPressed(object sender, PanelKeyEventArgs e)
		{
			_UserWants = UserAction.None;

			switch (e.Code)
			{
				case VKeyCode.Enter:
					{
						//! [CtrlEnter] is taken: insert the file name into the command line
						FarFile f = CurrentFile;
						if (f == null)
							return;
						switch (e.State)
						{
							case KeyStates.None:
								_UserWants = UserAction.Enter;
								if ((!IgnoreDirectoryFlag && f.IsDirectory) && AsOpenFile == null)
									return;
								e.Ignore = true;
								UIOpenFile(f);
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								UIAttributes();
								return;
						}
						return;
					}
				case VKeyCode.F1:
					{
						switch (e.State)
						{
							case KeyStates.None:
								e.Ignore = true;
								UIHelp();
								return;
						}
						return;
					}
				case VKeyCode.F3:
					{
						switch (e.State)
						{
							case KeyStates.None:
								if (CurrentFile == null)
								{
									e.Ignore = true;
									UIViewAll();
								}
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								ShowMenu();
								return;
						}
						return;
					}
				case VKeyCode.F5:
					{
						switch (e.State)
						{
							case KeyStates.None:
								e.Ignore = true;
								UICopyMove(false);
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								UICopyHere();
								return;
						}
						return;
					}
				case VKeyCode.F6:
					{
						switch (e.State)
						{
							case KeyStates.None:
								e.Ignore = true;
								UICopyMove(true);
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								UIRename();
								return;
						}
						return;
					}
				case VKeyCode.F7:
					{
						switch (e.State)
						{
							case KeyStates.None:
								e.Ignore = true;
								UICreate();
								return;
						}
						return;
					}
				case VKeyCode.PageDown:
					{
						switch (e.State)
						{
							case KeyStates.Control:
								e.Ignore = true;
								UIOpenFileMembers();
								return;
						}
						return;
					}
				case VKeyCode.A:
					{
						switch (e.State)
						{
							case KeyStates.Control:
								e.Ignore = true;
								UIAttributes();
								return;
						}
						return;
					}
				case VKeyCode.G:
					switch (e.State)
					{
						case KeyStates.Control:
							e.Ignore = true;
							UIApply();
							return;
					}
					return;
				case VKeyCode.M:
					switch (e.State)
					{
						case KeyStates.Control | KeyStates.Shift:
							e.Ignore = true;
							UIMode();
							return;
					}
					return;
				case VKeyCode.R:
					{
						// Ctrl
						switch (e.State)
						{
							case KeyStates.Control:
								_UserWants = UserAction.CtrlR;
								return;
						}
						return;
					}
				case VKeyCode.S:
					{
						// Ctrl
						switch (e.State)
						{
							case KeyStates.Control:
								e.Ignore = true;
								SaveData();
								return;
						}
						return;
					}
			}
		}

		/// <summary>
		/// Far event.
		/// </summary>
		void OnSetDirectory(object sender, SetDirectoryEventArgs e)
		{
			// *) .Find: is not used, ignore
			// *) .Silent: 100127 CtrlQ mode is not OK for folders: FarMacro: on areas Far tries to enumerate, we do not support. ???
			//???? _110121_150249 Time to do this has come.
			if ((e.Mode & (ExplorerModes.Find | ExplorerModes.Silent)) > 0)
			{
				e.Ignore = true;
				return;
			}

			// pop parent
			if (Parent != null && (e.Name == ".." || e.Name == "\\"))
			{
				e.Ignore = true;
				CloseChild();
				return;
			}

			// recall
			OnSetDirectory(e);
		}

		/// <summary>
		/// Virtual event.
		/// </summary>
		internal virtual void OnSetDirectory(SetDirectoryEventArgs e)
		{ }

		/// <summary>
		/// Returns e.g. MyDrive:
		/// </summary>
		internal static string SelectDrivePrompt(string select) //????
		{
			IMenu m = Far.Net.CreateMenu();
			m.AutoAssignHotkeys = true;
			m.Title = "Power panel";
			m.HelpTopic = A.Psf.HelpTopic + "MenuPanels";
			m.Add("Folder &tree");
			m.Add("&Any objects");
			m.Add(string.Empty).IsSeparator = true;
			int i = 2;
			foreach (object o in A.Psf.InvokeCode("Get-PowerShellFarDriveName"))
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
		internal static string SelectShare(string computer) //????
		{
			const string code = @"
Get-WmiObject -Class Win32_Share -ComputerName $args[0] |
Sort-Object Name |
.{process{ $_.Name; $_.Description }}
";
			Collection<PSObject> values = A.Psf.InvokeCode(code, computer);

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
		/// Called to delete the files.
		/// </summary>
		internal virtual void DoDeleteFiles(FilesEventArgs args) { }

		EventHandler<FileEventArgs> _LookupCloser;
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
		public void SetLookup(EventHandler<FileEventArgs> handler)
		{
			_LookupCloser = handler;
		}

		/// <summary>
		/// Opens <see cref="MemberPanel"/> for a file.
		/// File <c>Data</c> must not be null.
		/// </summary>
		internal virtual MemberPanel OpenFileMembers(FarFile file)
		{
			//??? _090610_071700, + $panel.SetOpen({ @ Test-Panel-Tree-.ps1
			object it = file.Data == null ? file : file.Data;
			MemberPanel r = new MemberPanel(it);
			r._LookupOpeners = _LookupOpeners;
			//! use null as parent: this panel can be not open now
			r.OpenChild(null);
			return r;
		}

		UserAction _UserWants;
		/// <summary>
		/// The last user action.
		/// </summary>
		internal UserAction UserWants
		{
			get { return _UserWants; }
		}

		/// <summary>
		/// Default writer.
		/// </summary>
		internal virtual void WriteFile(FarFile file, string path)
		{
			const string code = @"
Format-List -InputObject $args[0] -Property * -Expand Both -ErrorAction SilentlyContinue |
Out-File -FilePath $args[1] -Width $args[2]
";
			A.Psf.InvokeCode(code, file.Data, path, int.MaxValue);
		}

		/// <include file='doc.xml' path='doc/AddLookup/*'/>
		public void AddLookup(string name, EventHandler<FileEventArgs> handler)
		{
			if (_LookupOpeners == null)
				_LookupOpeners = new Dictionary<string, EventHandler<FileEventArgs>>();
			_LookupOpeners.Add(name, handler);
		}
		internal Dictionary<string, EventHandler<FileEventArgs>> _LookupOpeners;

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

	}
}
