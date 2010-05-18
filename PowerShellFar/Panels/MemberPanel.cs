/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Text.RegularExpressions;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Object member panel, e.g. property list to view or edit values.
	/// </summary>
	public sealed class MemberPanel : ListPanel
	{
		PSObject _Value;
		int _mode;

		/// <summary>
		/// New panel with an object.
		/// </summary>
		/// <param name="instance">An object which members are shown.</param>
		public MemberPanel(object instance)
		{
			// check not null and not a panel related instance
			if (instance == null)
				throw new ArgumentNullException("instance");
			if (instance is IAnyPanel || instance is AnyPanel)
				throw new ArgumentException("The object is a panel itself, its members can not be shown in a panel.");
			_Value = PSObject.AsPSObject(instance);
			if (_Value.BaseObject == null)
				throw new ArgumentNullException("instance");

			// panel info
			Panel.Info.CurrentDirectory = "*";
			Panel.Info.StartSortMode = PanelSortMode.Unsorted;
		}

		///
		protected override string DefaultTitle { get { return "Members: " + _Value.BaseObject.GetType().Name; } }

		internal override PSObject Target
		{
			get { return _Value; }
		}

		/// <summary>
		/// Gets or sets data modification flag.
		/// </summary>
		/// <remarks>
		/// It is set internally on any interactive data changes.
		/// If data are changed externally the this flag should be set, too.
		/// If this flag is set the panel asks you to save modified data and calls
		/// a script set by <see cref="SetSave"/>.
		/// </remarks>
		public bool Modified { get; set; }

		/// <summary>
		/// Tells to open an object panel with this <see cref="Value"/> on 'dots'.
		/// </summary>
		public bool ObjectPanelOnDots { get; set; }

		/// <summary>
		/// Object which member list is shown at the panel.
		/// </summary>
		public PSObject Value
		{
			get { return _Value; }
		}

		/// <summary>
		/// Tells not to create or delete members even if it is possible.
		/// </summary>
		public bool Static { get; set; }

		Regex GetExcludeMemberRegex()
		{
			try
			{
				if (!string.IsNullOrEmpty(ExcludeMemberPattern))
					return new Regex(ExcludeMemberPattern, RegexOptions.IgnoreCase);

				TablePanel table = Parent as TablePanel;
				if (table != null && !string.IsNullOrEmpty(table.ExcludeMemberPattern))
					return new Regex(table.ExcludeMemberPattern, RegexOptions.IgnoreCase);

				return null;
			}
			catch (Exception ex)
			{
				throw new ModuleException("Invalid exclude member pattern.", ex);
			}
		}

		Regex GetHideMemberRegex()
		{
			try
			{
				if (!string.IsNullOrEmpty(HideMemberPattern))
					return new Regex(HideMemberPattern, RegexOptions.IgnoreCase);

				TablePanel table = Parent as TablePanel;
				if (table != null && !string.IsNullOrEmpty(table.HideMemberPattern))
					return new Regex(table.HideMemberPattern, RegexOptions.IgnoreCase);

				return null;
			}
			catch (Exception ex)
			{
				throw new ModuleException("Invalid hide member pattern.", ex);
			}
		}

		internal override void OnGettingData(PanelEventArgs e)
		{
			try
			{
				Panel.Files.Clear();

				if (_mode == 0)
				{
					//! _100426_034702
					//! Get actual members to show:
					//! _Value.Properties contains too much, i.e. for DataRow: HasErrors, ItemArray, RowError, RowState;
					//! use Get-Member at first (don't use it itself, because it returns sorted data, we want unsorted).
					//! Idea to cache them is not good:
					//! price: high (have to sync on exclude, add, delete, etc.)
					//! value: low (it is UI and member number is normally small)
					var membersToShow = new List<string>();
					{
						Regex exclude = GetExcludeMemberRegex();
						string code = "Get-Member -InputObject $args[0] -MemberType Properties -ErrorAction 0";
						foreach (PSObject o in A.Psf.InvokeCode(code, _Value))
						{
							string name = o.Properties[Word.Name].Value.ToString();
							if (exclude == null || !exclude.IsMatch(name))
								membersToShow.Add(name);
						}
					}

					// now we are ready to process properties in their original order
					Regex hide = GetHideMemberRegex();
					foreach (PSPropertyInfo pi in _Value.Properties)
					{
						if (!membersToShow.Contains(pi.Name))
							continue;

						//! exceptions, e.g. exit code of running process
						object value;
						try
						{
							value = pi.Value;
						}
						catch (RuntimeException)
						{
							continue;
						}

						SetFile file = new SetFile()
						{
							Name = pi.Name,
							Data = pi
						};

						// base object
						PSObject asPSObject = value as PSObject;
						if (asPSObject != null)
							value = asPSObject.BaseObject;

						// value
						file.Description = Converter.FormatValue(value, A.Psf.Settings.FormatEnumerationLimit);

						// hidden
						if (hide != null && hide.IsMatch(file.Name))
							file.IsHidden = true;

						Panel.Files.Add(file);
					}
				}
				else
				{
					string code;
					if (_mode == 1)
						code = "Get-Member -InputObject $args[0] -ErrorAction 0 -View All";
					else
						code = "Get-Member -InputObject $args[0] -ErrorAction 0 -View All -Static";
					foreach (PSObject o in A.Psf.InvokeCode(code, _Value))
					{
						SetFile f = new SetFile();
						f.Name = o.Properties[Word.Name].Value.ToString();

						PSPropertyInfo pi;
						pi = o.Properties["MemberType"];
						f.Description = pi.Value.ToString();

						pi = o.Properties["Definition"];
						f.Description += " " + pi.Value.ToString();

						f.Data = o;
						Panel.Files.Add(f);
					}
				}
			}
			catch (RuntimeException exception)
			{
				if ((e.Mode & OperationModes.FindSilent) == 0)
					A.Msg(exception.Message);
			}
		}

		/// <summary>
		/// This method is called only because there is no parent,
		/// thus, it opens a new object panel with this one object.
		/// </summary>
		internal override void OnSettingDirectory(SettingDirectoryEventArgs e)
		{
			if (ObjectPanelOnDots)
			{
				e.Ignore = true;
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(new object[] { _Value });
				op.Show();
			}
		}

		internal override MemberPanel OpenFileMembers(FarFile file)
		{
			PSObject o = PSObject.AsPSObject(file.Data);
			string memberType = o.Properties["MemberType"].Value.ToString();
			if (!memberType.EndsWith("Property", StringComparison.Ordinal)) //??
				return null;

			string name = o.Properties[Word.Name].Value.ToString();
			object instance = _Value.Properties[name].Value;
			if (instance == null)
				return null;

			MemberPanel r = new MemberPanel(instance);
			r.ShowAsChild(this);
			return r;
		}

		internal override void UICreate()
		{
			if (Static)
				return;

			if (Parent is DataPanel)
				return;

			UI.NewValueDialog ui = new UI.NewValueDialog("New property");
			while (ui.Dialog.Show())
			{
				try
				{
					// get value, typed if needed
					object value = null;
					if (ui.Type.Text.Length == 0)
					{
						value = ui.Value.Text;
					}
					else
					{
						foreach (PSObject o in A.Psf.InvokeCode("[" + ui.Type.Text + "]$args[0]", ui.Value.Text))
						{
							value = o.BaseObject;
							break;
						}
					}

					// add member
					A.Psf.InvokeCode("$args[0] | Add-Member -MemberType NoteProperty -Name $args[1] -Value $args[2] -Force -ErrorAction Stop", _Value, ui.Name.Text, value);

					// update this panel with name
					UpdateRedraw(false, ui.Name.Text);

					// update that panel if the instance is the same
					MemberPanel pp2 = AnotherPanel as MemberPanel;
					if (pp2 != null && pp2._Value == _Value)
						pp2.UpdateRedraw(true);

					// exit the loop
					return;
				}
				catch (RuntimeException exception)
				{
					A.Msg(exception.Message);
					continue;
				}
			}
		}

		/// <summary>
		/// Should be called when an item property is changed.
		/// </summary>
		internal static void WhenMemberChanged(object instance)
		{
			MemberPanel p = Far.Net.FindPanel(typeof(MemberPanel)).Host as MemberPanel;
			if (p == null)
				return;

			if (p._Value == instance)
			{
				p.Modified = true;
				p.UpdateRedraw(true);
			}

			p = p.AnotherPanel as MemberPanel;
			if (p == null)
				return;

			if (p._Value == instance)
			{
				p.Modified = true;
				p.UpdateRedraw(true);
			}
		}

		/// <summary>
		/// Changes modes: properties, members, static
		/// </summary>
		internal override void UIMode()
		{
			if (++_mode > 1) // don't use static for now
				_mode = 0;
			UpdateRedraw(false, 0, 0);
		}

		/// <summary>
		/// Creates a lookup handler designed only for <see cref="DataRow"/>.
		/// </summary>
		/// <param name="namePairs">Destination and source field name pairs.</param>
		/// <returns>Lookup handler to be passed in <see cref="AnyPanel.SetLookup"/>.</returns>
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
		public EventHandler<FileEventArgs> CreateDataLookup(string[] namePairs)
		{
			if (Cast<DataRow>.From(_Value) == null)
				throw new InvalidOperationException("Data lookup is designed only for data row objects.");
			
			if (namePairs == null || namePairs.Length == 0)
				throw new ArgumentException("'namePairs' must not be null or empty.");
			
			if (namePairs.Length % 2 != 0)
				throw new ArgumentException("'namePairs' must contain even number of items.");

			return (new DataLookup(namePairs)).Invoke;
		}

		internal override void DeleteFiles(IList<FarFile> files, bool shift)
		{
			// skip "all members" mode
			if (_mode != 0)
				return;

			// delete value = enter null
			if (shift)
			{
				base.DeleteFiles(files, false);
				return;
			}

			// skip "static" mode
			if (Static)
				return;

			// skip data row
			if (Parent is DataPanel)
				return;

			if ((Far.Net.Confirmations & FarConfirmations.Delete) != 0)
			{
				if (Far.Net.Message("Delete selected members (if possible)?", Res.Delete, MsgOptions.None, new string[] { Res.Delete, Res.Cancel }) != 0)
					return;
			}

			try
			{
				foreach (FarFile f in files)
				{
					PSPropertyInfo pi = f.Data as PSPropertyInfo;
					if (pi == null)
						continue;
					_Value.Properties.Remove(pi.Name);
				}
			}
			finally
			{
				// update always, some members can be deleted, don't leave them dangling
				WhenMemberChanged(_Value);
			}
		}

		internal override void EditFile(FarFile file, bool alternative)
		{
			PSPropertyInfo pi = Cast<PSPropertyInfo>.From(file.Data);
			if (pi == null)
				return;

			string text = Converter.InfoToText(pi);
			if (text == null)
				return;

			try
			{
				string tmp = Far.Net.TempName();
				try
				{
					// warning
					if (!pi.IsSettable)
						A.Msg(Res.PropertyIsNotSettable);

					// internal editor:
					if (!alternative)
					{
						File.WriteAllText(tmp, text, Encoding.Unicode);
						MemberEditor edit = new MemberEditor();
						edit.Open(tmp, true, _Value, pi);
						tmp = null;
						return;
					}

					// notepad:
					for (; ; )
					{
						File.WriteAllText(tmp, text, Encoding.Unicode);
						DateTime stamp1 = File.GetLastWriteTime(tmp);
						My.ProcessEx.StartNotepad(tmp).WaitForExit();

						// exit if it is a read only property
						if (file.IsReadOnly)
							return;

						// exit if it is not modified
						DateTime stamp2 = File.GetLastWriteTime(tmp);
						if (stamp2 <= stamp1)
							return;

						try
						{
							object value;
							if (pi.TypeNameOfValue.EndsWith("]", StringComparison.Ordinal))
								value = File.ReadAllLines(tmp, Encoding.Unicode);
							else
								value = File.ReadAllText(tmp, Encoding.Unicode);

							A.SetMemberValue(pi, Converter.Parse(pi, value));
							WhenMemberChanged(_Value);
							break;
						}
						catch (RuntimeException ex)
						{
							A.Msg(ex);
						}
					}
				}
				finally
				{
					if (tmp != null)
						File.Delete(tmp);
				}
			}
			catch (RuntimeException ex)
			{
				Far.Net.ShowError("Edit", ex);
			}
		}

		/// <summary>
		/// Calls one of:
		/// *) a handler if it is set by <see cref="SetSave"/>;
		/// *) <c>Parent.SaveData()</c> if it is a child panel.
		/// </summary>
		public override bool SaveData()
		{
			if (_Save != null)
			{
				InvokeScriptReturnAsIs(_Save, null);
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
					A.SetMemberValue(info, null);
				else
					//! it is tempting to avoid our parsing, but it is not that good..
					A.SetMemberValue(info, Converter.Parse(info, value));

				// change is done
				WhenMemberChanged(_Value);
			}
			catch (RuntimeException ex)
			{
				A.Msg(ex);
			}
		}

		/// <summary>
		/// Regular expression pattern of members to be excluded.
		/// </summary>
		public string ExcludeMemberPattern { get; set; }

		/// <summary>
		/// Regular expression pattern of members to be hidden.
		/// </summary>
		public string HideMemberPattern { get; set; }

		/// <summary>
		/// Sets a handler called to save modified data.
		/// It has to save data and set <see cref="Modified"/> = $false.
		/// </summary>
		public void SetSave(ScriptBlock handler)
		{
			_Save = handler;
		}
		ScriptBlock _Save;

		internal override bool CanClose()
		{
			// can?
			bool r = !Modified || _Save == null;

			// ask
			if (!r)
			{
				switch (Far.Net.Message(Res.AskSaveModified, "Save", MsgOptions.YesNoCancel))
				{
					case 0:
						InvokeScriptReturnAsIs(_Save, null);
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
			if (items.Save == null && (_Save != null || Parent != null && (Parent is DataPanel)))
				items.Save = new SetItem()
				{
					Text = "Save data",
					Click = delegate { SaveData(); }
				};

			base.HelpMenuInitItems(items, e);
		}
	}
}
