/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Formatted table panel.
	/// </summary>
	public abstract class FormatPanel : TablePanel
	{
		internal FileMap Map; // internal ???

		/// <include file='doc.xml' path='docs/pp[@name="Columns"]/*'/>
		public override sealed object[] Columns
		{
			get
			{
				return base.Columns;
			}
			set
			{
				base.Columns = value;
				if (value == null)
					Map = null;
				else
					MakeMap(null);
			}
		}

		/// <summary>
		/// Same as <c>-AutoSize</c> of <c>Format-Table</c>.
		/// </summary>
		/// <remarks>
		/// Ignored if <see cref="Columns"/> is set.
		/// <para>
		/// This mode is slower but formatted data may look better.
		/// Sometimes table formatting possible only in auto size mode.
		/// </para>
		/// <para>
		/// If table formatting is not possible even in auto size mode
		/// then you should use manually specified <see cref="TablePanel.Columns"/>.
		/// </para>
		/// </remarks>
		public bool AutoSize { get; set; }

		///
		protected FormatPanel()
		{ }

		//! assume it is done for the active panel, it does not work well from the disk menu
		internal static Meta[] TryFormatByTableControl(PSObject value)
		{
			TableControl table = A.FindTableControl(value.BaseObject.GetType().FullName, null);
			if (table == null)
				return null;

			int count = Math.Min(table.Rows[0].Columns.Count, A.Psf.Settings.MaximumPanelColumnCount);
			Meta[] metas = new Meta[count];
			for (int i = 0; i < count; ++i)
				metas[i] = new Meta(table.Rows[0].Columns[i].DisplayEntry, table.Headers[i]);

			// adjust formatting to the panel width ????
			{
				int totalWidth = A.Far.Panel.Window.Width - (metas.Length + 1); // N columns ~ N + 1 borders
				int setSum = 0;
				int setCount = 0;
				int setMaxValue = 0;
				int setMaxIndex = -1;
				for (int i = metas.Length; --i >= 0; )
				{
					int width = metas[i].Width;
					if (width > 0)
					{
						++setCount;
						setSum += width;
						if (setMaxValue < width)
						{
							setMaxValue = width;
							setMaxIndex = i;
						}
					}
				}

				// panel is too narrow (less than 5 chars for unset columns), drop all positive widths
				if (setSum + (metas.Length - setCount) * 5 > totalWidth)
				{
					foreach (Meta meta in metas)
						if (meta.Width > 0)
							meta.Width = 0;
				}
				// panel is too wide (e.g. for Get-Service ~ 64), drop the maximum width
				else if (setCount == metas.Length && setSum < totalWidth)
				{
					metas[setMaxIndex].Width = 0;
				}
			}

			// set heuristic types
			SetBestTypes(metas);
			return metas;
		}

		internal static Meta[] TryFormatByGetMember(Collection<PSObject> values)
		{
			Meta[] metas;
			int count;

			if (values.Count == 1) //???? need
			{
				List<Meta> tmp = new List<Meta>();
				foreach (PSPropertyInfo pi in values[0].Properties)
				{
					tmp.Add(new Meta(pi.Name));
					if (tmp.Count >= A.Psf.Settings.MaximumPanelColumnCount)
						break;
				}

				count = tmp.Count;
				if (count == 0)
					return null;

				metas = new Meta[count];
				tmp.CopyTo(metas);
			}
			else
			{
				Collection<PSObject> members = A.Psf.InvokeCode("$args[0] | Get-Member -MemberType Property -ErrorAction 0 | Select-Object -ExpandProperty Name", values);
				if (members.Count == 0)
					return null;

				count = Math.Min(members.Count, A.Psf.Settings.MaximumPanelColumnCount);
				metas = new Meta[count];
				for (int i = 0; i < count; ++i)
					metas[i] = new Meta(members[i].ToString());
			}

			// set heuristic types
			SetBestTypes(metas);
			return metas;
		}

		static void SetBestTypes(Meta[] metas)
		{
			int count = metas.Length;
			
			// heuristic N
			if (count > 1 && SetBestType(metas, "N", Word.Name, "*" + Word.Name, Word.Id, Word.Key, "*" + Word.Key, "*" + Word.Id))
				--count;

			// heuristic Z
			if (count > 1 && SetBestType(metas, "Z", Word.Description, Word.Definition))
				--count;

			// heuristic O
			if (count > 1)
				SetBestType(metas, "O", Word.Value, Word.Status);
		}

		static bool SetBestType(Meta[] metas, string type, params string[] patterns)
		{
			int bestRank = patterns.Length;
			Meta bestMeta = null;

			foreach (Meta meta in metas)
			{
				if (meta.Type != null)
					continue;

				string name = meta.Name;
				for (int i = 0; i < bestRank; ++i)
				{
					string pattern = patterns[i];
					if (pattern[0] == '*')
					{
						if (name.EndsWith(patterns[i].Substring(1), StringComparison.OrdinalIgnoreCase))
						{
							bestRank = i;
							bestMeta = meta;
						}
					}
					else
					{
						if (string.Compare(name, patterns[i], StringComparison.OrdinalIgnoreCase) == 0)
						{
							if (i == 0)
							{
								meta.Type = type;
								return true;
							}

							bestRank = i;
							bestMeta = meta;
						}
					}
				}
			}

			if (bestMeta != null)
			{
				bestMeta.Type = type;
				return true;
			}

			return false;
		}

		void MakeMap(Meta[] metas) //????
		{
			// pass 1: get metas and types and pre-process only specified default types
			if (metas == null)
				metas = SetupColumns(Columns);
			else
				SetupMetas(metas);

			// pass 2: process all, use still available default column types
			Map = new FileMap();
			foreach (Meta meta in metas)
			{
				// type -> map:
				switch (meta.Type[0])
				{
					case 'N':
						Map.Name = meta;
						break;
					case 'O':
						Map.Owner = meta;
						break;
					case 'Z':
						Map.Description = meta;
						break;
					case 'C':
						Map.Columns.Add(meta);
						break;
					case 'S':
						{
							if (Map.Length != null)
								throw new InvalidOperationException("Column 'S' is used twice.");
							Map.Length = meta;
						}
						break;
					case 'D':
						{
							if (meta.Type.Length < 2)
								throw new InvalidOperationException("Invalid column type: D");

							switch (meta.Type[1])
							{
								case 'C':
									{
										if (Map.CreationTime != null)
											throw new InvalidOperationException("Column 'DC' is used twice.");

										Map.CreationTime = meta;
									}
									break;
								case 'M':
									{
										if (Map.LastWriteTime != null)
											throw new InvalidOperationException("Column 'DM' is used twice.");

										Map.LastWriteTime = meta;
									}
									break;
								case 'A':
									{
										if (Map.LastAccessTime != null)
											throw new InvalidOperationException("Column 'DA' is used twice.");

										Map.LastAccessTime = meta;
									}
									break;
								default:
									throw new InvalidOperationException("Invalid column type: " + meta.Type);
							}
						}
						break;
					default:
						throw new InvalidOperationException("Unknown column type: " + meta.Type);
				}
			}

			// pass 3: set panel mode
			Panel.Info.SetMode(PanelViewMode.AlternativeFull, SetupPanelMode(metas));
		}

		/// <summary>
		/// Gets a list of ready files or a collection of PS objects.
		/// </summary>
		internal abstract object GetData();

		/// <summary>
		/// Calls <see cref="GetData()"/> and then formats if needed.
		/// </summary>
		internal override void OnGettingData(PanelEventArgs e)
		{
			// call the worker
			// _090408_232925 If we throw then FarNet returns false and Far closes the panel.
			object data;
			try
			{
				data = GetData();
			}
			catch (RuntimeException ex)
			{
				if ((e.Mode & OperationModes.Silent) == 0)
					A.Far.ShowError(Res.Me, ex);

				data = new List<FarFile>();
			}

			// if the data are files just use them, assume all is done
			IList<FarFile> readyFiles = data as IList<FarFile>;
			if (readyFiles != null)
			{
				Panel.Files = readyFiles;
				return;
			}

			// PS objects
			Collection<PSObject> values = (Collection<PSObject>)data;

			// empty?
			if (values.Count == 0)
			{
				// drop files in any case
				Panel.Files.Clear();

				// do not change anything in the custom panel
				if (Columns != null)
					return;

				// is it already <empty>?
				PanelModeInfo mode = Panel.Info.GetMode(PanelViewMode.AlternativeFull);
				if (mode == null)
					mode = new PanelModeInfo();
				else if (mode.Columns.Length == 1 && mode.Columns[0].Name == "<empty>")
					return;

				// reuse the mode: reset columns, keep other data intact
				SetColumn c1 = new SetColumn();
				c1.Type = "N";
				c1.Name = "<empty>";
				mode.Columns = new FarColumn[] { c1 };
				Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode);
				return;
			}

			// not empty; values has to be removed in any case
			try
			{
				// custom
				if (Columns != null)
				{
					BuildFiles(values);
					return;
				}

				// the common type
				Type commonType = values[0].BaseObject is System.Collections.IEnumerable ? null : A.FindCommonType(values);

				// use index, value, type mode
				if (commonType == null)
				{
					BuildFilesMixed(values);
					return;
				}

				Meta[] metas = null;

				// try to get format
				if (commonType != typeof(PSCustomObject))
					metas = TryFormatByTableControl(values[0]);

				// use Get-Member
				if (metas == null)
					metas = TryFormatByGetMember(values);

				if (metas == null)
				{
					BuildFilesMixed(values);
				}
				else
				{
					MakeMap(metas);
					BuildFiles(values);
				}
			}
			finally
			{
				values.Clear();
			}
		}

		///
		protected virtual void BuildFiles(Collection<PSObject> values)
		{
			List<FarFile> files = new List<FarFile>(values.Count);
			Panel.Files = files;

			foreach (PSObject value in values)
				files.Add(new MapFile(value, Map));
		}

		void BuildFilesMixed(Collection<PSObject> values)
		{
			List<FarFile> files = new List<FarFile>(values.Count);
			Panel.Files = files;

			PanelModeInfo mode = Panel.Info.GetMode(PanelViewMode.AlternativeFull);
			if (mode == null)
				mode = new PanelModeInfo();
			SetColumn c1 = new SetColumn(); c1.Type = "S"; c1.Name = "##"; // "Index" clashes to sort order mark
			SetColumn c2 = new SetColumn(); c2.Type = "N"; c2.Name = "Value";
			SetColumn c3 = new SetColumn(); c3.Type = "Z"; c3.Name = "Type";
			mode.Columns = new FarColumn[] { c1, c2, c3 };
			Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode);

			int i = 0;
			foreach (PSObject value in values)
			{
				SetFile file = new SetFile();
				file.Data = value;
				file.Length = i++;
				file.Description = value.BaseObject.GetType().FullName;
				PSPropertyInfo pi = A.FindDisplayProperty(value);
				if (pi == null)
					file.Name = value.ToString();
				else
					file.Name = pi.Value.ToString();
				files.Add(file);
			}
		}

		internal override string HelpMenuTextOpenFileMembers { get { return "Object members"; } }

	}
}
