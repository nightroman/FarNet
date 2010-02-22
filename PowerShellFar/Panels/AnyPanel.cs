/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Base class of PowerShellFar panels.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class is a host of <see cref="IPanel"/>
	/// (see <see cref="Panel"/> and <see cref="IPanel.Host"/>).
	/// It manages extra panel data and "items" attached to panel "files"
	/// and implements panel event handlers for some basic panel operations.
	/// </para>
	/// <para>
	/// Terminology (both for names and documentation):
	/// "files" (<c>FarFile</c>) are elements of <see cref="IPanel"/>;
	/// "items" (<c>PSObject</c>) are <see cref="FarFile.Data"/> attached to "files".
	/// Note that null and ".." items are not processed (e.g. by <see cref="ShownItems"/>
	/// and <see cref="SelectedItems"/>).
	/// </para>
	/// </remarks>
	public abstract partial class AnyPanel
	{
		/// <summary>
		/// Default panel.
		/// </summary>
		internal AnyPanel()
		{
			// create a panel
			_Panel = Far.Net.CreatePanel();

			// set host, etc.
			_Panel.Host = this;
			_Panel.AddDots = true;

			// add event handlers
			_Panel.Closed += OnClosed;
			_Panel.Executing += OnExecuting;
			_Panel.GettingData += OnGettingData;
			_Panel.GettingFiles += OnGettingFiles;
			_Panel.Idled += OnIdled;
			_Panel.KeyPressed += OnKeyPressed;
			_Panel.SettingDirectory += OnSettingDirectory;

			// auto names
			_Panel.Info.AutoAlternateNames = true;

			// start mode
			_Panel.Info.StartViewMode = PanelViewMode.AlternativeFull;
		}

		/// <summary>
		/// Gets the default panel title to be set on show.
		/// </summary>
		protected virtual string DefaultTitle { get { return GetType().Name; } }

		AnyPanel _Child;
		/// <summary>
		/// Child panel.
		/// </summary>
		internal AnyPanel Child
		{
			get { return _Child; }
		}

		/// <summary>
		/// Gets or sets user data attached to this panel.
		/// </summary>
		/// <remarks>
		/// This is just a shortcut reference to <see cref="IPanel.Data"/>, not another instance.
		/// </remarks>
		public object Data
		{
			get { return _Panel.Data; }
			set { _Panel.Data = value; }
		}

		List<IDisposable> _Garbage;
		/// <summary>
		/// Gets the list of user objects that have to be disposed when the panel is closed.
		/// </summary>
		public IList<IDisposable> Garbage
		{
			get
			{
				if (_Garbage == null)
					_Garbage = new List<IDisposable>();
				return _Garbage;
			}
		}

		//! this.Data is connected to Panel.Data
		readonly IPanel _Panel;
		/// <summary>
		/// Gets the hosted panel.
		/// </summary>
		/// <remarks>
		/// The hosted panel refers to this as <see cref="IPanel.Host"/>.
		/// </remarks>
		public IPanel Panel
		{
			get { return _Panel; }
		}

		AnyPanel _Parent;
		/// <summary>
		/// Gets the parent panel.
		/// </summary>
		/// <remarks>
		/// The parent panel is null if this panel is not a child panel.
		/// </remarks>
		public AnyPanel Parent
		{
			get { return _Parent; }
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
		/// Thus, this set set is not the same as <see cref="IPanel.Files"/>.
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
				foreach (FarFile file in _Panel.ShownFiles)
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
				foreach (FarFile file in _Panel.SelectedFiles)
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
				return ConvertFileToItem(_Panel.CurrentFile);
			}
		}

		/// <summary>
		/// Tells to treat the items as not directories even if they have a directory flag.
		/// </summary>
		internal bool IgnoreDirectoryFlag { get; set; } // _090810_180151

		/// <summary>
		/// Gets active or passive PowerShellFar panel if any.
		/// </summary>
		/// <param name="active">Active or passive panel.</param>
		/// <returns>Found panel or null.</returns>
		public static AnyPanel GetPanel(bool active)
		{
			IPanel plug = Far.Net.FindPanel(typeof(AnyPanel));
			if (plug == null)
				return null;
			AnyPanel pp = plug.Host as AnyPanel;
			if (pp._Panel.IsActive == active)
				return pp;
			return pp.AnotherPanel;
		}

		/// <summary>
		/// It is called to save data.
		/// By default it just returns true.
		/// </summary>
		/// <returns>true if there is no more data to save.</returns>
		public virtual bool SaveData()
		{
			return true;
		}

		/// <summary>
		/// Saves current panel state.
		/// This version saves a file name; it is the least effective but:
		/// *) indexes may change (when items added|removed)
		/// *) panel files can be recreated on getting data.
		/// </summary>
		internal virtual void SaveState()
		{
			FarFile f = _Panel.CurrentFile;
			if (f != null)
				_Panel.PostName(f.Name);
		}

		// actual show
		void Open(object sender, EventArgs e)
		{
			if (_Parent == null)
				_Panel.Open();
			else
				_Panel.Open(_Parent._Panel);
		}

		/// <summary>
		/// Tells to shows the panel. It is OK to call it more than once.
		/// The panel is actually shown only when Far gets control.
		/// Don't call from a modal dialog, editor or viewer.
		/// </summary>
		/// <seealso cref="Show(bool)"/>
		/// <seealso cref="ShowAsChild"/>
		public virtual void Show()
		{
			// done
			if (_Panel.IsOpened)
				return;

			// set the title to default
			if (string.IsNullOrEmpty(_Panel.Info.Title))
				_Panel.Info.Title = DefaultTitle;

			// try to open even not from panels
			WindowKind wt = Far.Net.WindowKind;
			if (wt != WindowKind.Panels)
			{
				try
				{
					Far.Net.SetCurrentWindow(0);
				}
				catch (InvalidOperationException e)
				{
					throw new ModuleException("Cannot open a panel because panels window cannot be set current.", e);
				}

				// 090623 PostJob may not work from the editor, for example, see "... because a module is not called for opening".
				// I tried to ignore my check - a panel did not open. In contrast, PostStep calls via the menu where
				// a panel is opened from with no problems.
				Far.Net.PostStep(Open);
				return;
			}

			Open(null, null);
		}

		/// <summary>
		/// Calls <see cref="Show()"/> or <see cref="ShowAsChild"/> (child of the active panel) depending on a parameter.
		/// </summary>
		public void Show(bool child)
		{
			if (child)
				ShowAsChild(null);
			else
				Show();
		}

		/// <summary>
		/// Shows this panel as a child of a parent panel.
		/// It sets parent/child relation and calls <see cref="Show()"/>.
		/// </summary>
		/// <param name="parent">
		/// Parent panel (must be shown at this moment).
		/// Null tells to use the active PSF panel, if any.
		/// </param>
		/// <remarks>
		/// When this panel is shown as a child of its parent panel, the parent
		/// panel is not closed and normally it is shown again later when the
		/// child is closed.
		/// </remarks>
		public void ShowAsChild(AnyPanel parent)
		{
			// resolve 'null' parent
			if (parent == null)
			{
				// use the active as parent
				parent = GetPanel(true);

				// 091103 Do not try to use the passive panel and do not throw, why? Show as normal.
				if (parent == null)
				{
					// go
					Show();
					return;
				}
			}

			// sanity
			if (_Panel.IsOpened || _Parent != null)
				throw new InvalidOperationException();

			// lookup: try to post the current
			if (_LookupCloser != null)
			{
				// 090809 ??? perhaps I have to rethink
				TablePanel tp = this as TablePanel;
				if (tp != null)
				{
					// assume parent Description = child Name
					string value = parent._Panel.CurrentFile.Description;
					_Panel.PostName(value);
				}
			}

			// link
			_Parent = parent;
			_Parent._Child = this;

			// begin
			_Parent.SaveState();

			// go
			Show();
		}

		/// <summary>
		/// Shows help.
		/// </summary>
		internal virtual void ShowHelp()
		{
			Far.Net.ShowHelp(A.Psf.AppHome, "PowerPanel", HelpOptions.Path);
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
		/// Shows a parent panel and closes this.
		/// </summary>
		internal void ShowParent()
		{
			if (_Parent == null)
				throw new InvalidOperationException("Parent is null.");

			if (!CanClose())
				return;

			if (_Parent != null && !_Parent.CanCloseChild())
				return;

			// open parent
			_Parent._Panel.Open(_Panel);

			// unlink
			_Parent._Child = null;
			_Parent = null;
		}

		///
		public override string ToString()
		{
			return GetType().Name;
		}

		/// <summary>
		/// Can the panel close now?
		/// </summary>
		/// <remarks>
		/// NOTE: it can be called from a child; in this case the panel is offline.
		/// </remarks>
		internal virtual bool CanClose()
		{
			return true;
		}

		/// <summary>
		/// Can the parent panel close its child?
		/// </summary>
		internal virtual bool CanCloseChild()
		{
			return true;
		}

		/// <summary>
		/// Files data: .. is excluded; same count and order.
		/// </summary>
		internal IList<object> CollectData()
		{
			List<object> r = new List<object>();
			r.Capacity = _Panel.Files.Count;
			foreach (FarFile f in _Panel.Files)
				if (f.Data != null)
					r.Add(f.Data);
			return r;
		}

		/// <summary>
		/// Collects names of files.
		/// </summary>
		internal static IList<string> CollectNames(IList<FarFile> files)
		{
			List<string> r = new List<string>();
			r.Capacity = files.Count;
			foreach (FarFile f in files)
				r.Add(f.Name);
			return r;
		}

		/// <summary>
		/// Collects original names (selected if any or the current).
		/// </summary>
		internal IList<string> CollectSelectedNames()
		{
			List<string> r = new List<string>();
			foreach (FarFile f in _Panel.SelectedFiles)
				r.Add(f.Name);
			return r;
		}

		/// <summary>
		/// Far handler.
		/// </summary>
		internal abstract void OnGettingData(PanelEventArgs e);
		void OnGettingData(object sender, PanelEventArgs e)
		{
			OnGettingData(e);
		}

		/// <summary>
		/// Far handler. Actually used only for quick view of a file.
		/// </summary>
		void OnGettingFiles(object sender, GettingFilesEventArgs e)
		{
			if ((e.Mode & OperationModes.QuickView) == 0 || e.Files.Count != 1)
				return;

			FarFile file = e.Files[0];
			if (file.Data == null)
				return;

			//! file.AlternateName must be null as we auto them
			WriteFile(file, My.PathEx.Combine(e.Destination, e.Names[0]));
		}

		/// <summary>Attributes action.</summary>
		internal virtual void UIAttributes() { }

		/// <summary>Command action.</summary>
		/// <returns>True if handled.</returns>
		internal virtual bool UICommand(string code)
		{
			return false;
		}

		/// <summary>Create action.</summary>
		internal virtual void UICreate() { }

		/// <summary>Copy here action.</summary>
		internal virtual void UICopyHere() { }

		internal virtual bool UICopyMoveCan(bool move)
		{
			return !move && AnotherPanel is ObjectPanel;
		}

		/// <summary>
		/// Copy\move action.
		/// </summary>
		/// <returns>Processed?</returns>
		/// <remarks>
		/// This simply copies items to another <see cref="ObjectPanel"/> if any.
		/// </remarks>
		internal virtual bool UICopyMove(bool move)
		{
			if (move)
				return false;

			ObjectPanel op = AnotherPanel as ObjectPanel;
			if (op == null)
				return false;

			op.AddObjects(SelectedItems);
			UpdateRedraw(false);
			return true;
		}

		/// <summary>Delete action.</summary>
		internal void UIDelete(bool shift)
		{
			IList<FarFile> ff = _Panel.SelectedFiles;
			if (ff.Count == 0)
				return;

			DeleteFiles(ff, shift);
			UpdateRedraw(false);
		}

		/// <summary>
		/// Escape handler.
		/// </summary>
		internal virtual void UIEscape(bool all)
		{
			if (!CanClose())
				return;

			if (all || _Parent == null)
			{
				// _090321_210416 We do not call Redraw(0, 0) to reset cursor to 0 any more.
				// See Mantis 1114: why it was needed. Now FarNet panels restore original state.

				// ask parents
				if (all)
				{
					for (AnyPanel parent = _Parent; parent != null; parent = parent._Parent)
						if (!parent.CanClose())
							return;
				}

				// close
				_Panel.Close();
			}
			else
			{
				ShowParent();
			}
		}

		/// <summary>
		/// Shows help or the panel menu.
		/// </summary>
		internal void UIHelp()
		{
			ShowMenu();
		}

		/// <summary>
		/// Inserts text into the command line.
		/// </summary>
		internal virtual bool UIInsert()
		{
			return false;
		}

		/// <summary>Mode action.</summary>
		internal virtual void UIMode() { }

		///
		internal void UIOpenFileMembers()
		{
			FarFile f = _Panel.CurrentFile;
			if (f != null)
				OpenFileMembers(f);
		}

		/// <summary>Rename action.</summary>
		internal virtual void UIRename() { }

		// Far handler
		void OnIdled(object sender, EventArgs e)
		{ }

		// Far handler
		void OnKeyPressed(object sender, PanelKeyEventArgs e)
		{
			_UserWants = UserAction.None;

			switch (e.Code)
			{
				case VKeyCode.Enter:
					{
						if (e.Preprocess)
							return;
						FarFile f = _Panel.CurrentFile;
						if (f == null)
							return;
						switch (e.State)
						{
							case KeyStates.None:
								_UserWants = UserAction.Enter;
								if ((!IgnoreDirectoryFlag && f.IsDirectory) && _Open == null)
									return;
								e.Ignore = true;
								UIOpenFile(f);
								return;
							case KeyStates.Control:
								e.Ignore = UIInsert();
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
								e.Ignore = true;
								UIView();
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								ShowMenu();
								return;
						}
						return;
					}
				case VKeyCode.F4:
					{
						switch (e.State)
						{
							case KeyStates.None:
								e.Ignore = true;
								UIEditFile(_Panel.CurrentFile, false);
								return;
							case KeyStates.Alt:
								e.Ignore = true;
								UIEditFile(_Panel.CurrentFile, true);
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
				case VKeyCode.Delete:
					{
						if (Far.Net.CommandLine.Length > 0)
							return;
						goto case VKeyCode.F8;
					}
				case VKeyCode.F8:
					{
						switch (e.State)
						{
							case KeyStates.None:
								e.Ignore = true;
								UIDelete(false);
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								UIDelete(true);
								return;
						}
						return;
					}
				case VKeyCode.Escape:
					{
						switch (e.State)
						{
							case KeyStates.None:
								if (Far.Net.CommandLine.Length > 0)
									return;
								e.Ignore = true;
								UIEscape(false);
								return;
							case KeyStates.Shift:
								e.Ignore = true;
								UIEscape(true);
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
		void OnSettingDirectory(object sender, SettingDirectoryEventArgs e)
		{
			// *) .Find: is not used, ignore
			// *) .Silent: 100127 CtrlQ mode is not OK for folders: FarMacro: on areas Far tries to enumerate, we do not support. ???
			if ((e.Mode & (OperationModes.Find | OperationModes.Silent)) > 0)
			{
				e.Ignore = true;
				return;
			}

			// pop parent
			if (Parent != null && (e.Name == ".." || e.Name == "\\"))
			{
				e.Ignore = true;
				ShowParent();
				return;
			}

			// recall
			OnSettingDirectory(e);
		}

		/// <summary>
		/// Virtual event.
		/// </summary>
		internal virtual void OnSettingDirectory(SettingDirectoryEventArgs e) { }

		/// <summary>
		/// Returns e.g. MyDrive:
		/// </summary>
		internal static string SelectDrivePrompt(string select)
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
		internal static string SelectShare(string computer)
		{
			string code = @"
Get-WmiObject -Class win32_share -ComputerName $args[0] | Sort-Object Name | .{process{
$_.Name
$_.Description
}}";
			Collection<PSObject> oo = A.Psf.InvokeCode(code, computer);

			IMenu m = Far.Net.CreateMenu();
			m.AutoAssignHotkeys = true;
			m.Title = computer + " shares";
			for (int i = 0; i < oo.Count; i += 2)
			{
				string name = oo[i].ToString();
				string desc = oo[i + 1].ToString();
				if (desc.Length > 0)
					name += " (" + desc + ")";
				m.Add(name);
			}
			if (!m.Show())
				return null;

			return oo[2 * m.Selected].ToString();
		}

		/// <summary>
		/// Updates Far data and redraws.
		/// </summary>
		internal void UpdateRedraw(bool keepSelection)
		{
			_Panel.Update(keepSelection);
			_Panel.Redraw();
		}

		/// <summary>
		/// Updates Far data and redraws with positions.
		/// </summary>
		internal void UpdateRedraw(bool keepSelection, int current, int top)
		{
			_Panel.Update(keepSelection);
			_Panel.Redraw(current, top);
		}

		/// <summary>
		/// Updates Far data and redraws.
		/// </summary>
		internal void UpdateRedraw(bool keepSelection, string setName)
		{
			_Panel.Update(keepSelection);
			_Panel.PostName(setName);
			_Panel.Redraw();
		}

		/// <summary>
		/// Gets another started Power panel or null. Assume 'this' is a started panel.
		/// </summary>
		internal AnyPanel AnotherPanel
		{
			get
			{
				IPanel pp = _Panel.AnotherPanel;
				if (pp != null && pp.Host is AnyPanel)
					return pp.Host as AnyPanel;
				return null;
			}
		}

		/// <summary>
		/// Far handler.
		/// </summary>
		void OnClosed(object sender, EventArgs e)
		{
			// notify the parent firstly
			if (_Parent != null)
			{
				_Parent.OnClosed();
				_Parent._Child = null;
				_Parent = null;
			}

			// notify this (child)
			OnClosed();

			// garbage
			if (_Garbage != null)
			{
				foreach (IDisposable o in _Garbage)
					o.Dispose();
				_Garbage = null;
			}
		}

		/// <summary>
		/// Called by Far handler.
		/// </summary>
		internal virtual void OnClosed()
		{
			// notify the parents
			if (_Parent != null)
				_Parent.OnClosed();
		}

		/// <summary>
		/// Far handler.
		/// </summary>
		void OnExecuting(object sender, ExecutingEventArgs e)
		{
			// invoke virtual
			e.Ignore = UICommand(e.Command);

			// clean the line
			if (e.Ignore)
				Far.Net.CommandLine.Text = string.Empty;
		}

		/// <summary>
		/// Called to delete the files.
		/// </summary>
		internal virtual void DeleteFiles(IList<FarFile> files, bool shift) { }

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
			r.ShowAsChild(null);
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
			using (PowerShell p = A.Psf.CreatePipeline())
			{
				Command c = new Command("Format-List");
				c.Parameters.Add("InputObject", file.Data);
				c.Parameters.Add("Property", "*");
				c.Parameters.Add("Expand", "Both");
				c.Parameters.Add(Prm.EASilentlyContinue);
				p.Commands.AddCommand(c);
				c = new Command("Out-File");
				c.Parameters.Add("FilePath", path);
				c.Parameters.Add("Width", int.MaxValue);
				p.Commands.AddCommand(c);
				p.Invoke();
			}
		}

		internal Dictionary<string, EventHandler<FileEventArgs>> _LookupOpeners;
		/// <include file='doc.xml' path='docs/pp[@name="AddLookup"]/*'/>
		public void AddLookup(string name, EventHandler<FileEventArgs> handler)
		{
			if (_LookupOpeners == null)
				_LookupOpeners = new Dictionary<string, EventHandler<FileEventArgs>>();
			_LookupOpeners.Add(name, handler);
		}

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
			PanelMenuEventArgs e = new PanelMenuEventArgs(r, Panel.CurrentFile, Panel.SelectedList);

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
			{
				items.Exit = new SetItem();
				items.Exit.Text = "E&xit panel";
				items.Exit.Click = delegate { Panel.Close(); };
			}

			if (items.Help == null)
			{
				items.Help = new SetItem();
				items.Help.Text = "Help (F1)";
				items.Help.Click = delegate { ShowHelp(); };
			}
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
