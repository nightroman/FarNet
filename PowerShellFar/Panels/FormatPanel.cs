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
		internal ObjectFileMap Map; // internal ???
		internal bool FromGettingData { get; private set; } // internal ???

		/// <summary>
		/// Columns to include. Set it only when the panel has no files.
		/// </summary>
		/// <remarks>
		/// Items are similar to <c>-Property</c> of <c>Format-Table</c>.
		/// See more: <see cref="TablePanel.Columns"/> and <see cref="Meta"/>.
		/// </remarks>
		public override object[] Columns
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
					MakeMap();
			}
		}

		/// <summary>
		/// Name of a property for Name column.
		/// </summary>
		public Meta FarName { get; set; }

		/// <summary>
		/// Columns to exclude.
		/// </summary>
		/// <remarks>
		/// It is ignored if <see cref="TablePanel.Columns"/> is set.
		/// Items are the same as for <c>-ExcludeProperty</c> of <c>Select-Object</c> cmdlet.
		/// <para>
		/// It is not really recommended to use. Recommended way is to specify <see cref="Columns"/>.
		/// </para>
		/// </remarks>
		public string[] ExcludeColumns { get; set; }

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

		///
		public override void Show()
		{
			// done?
			if (Panel.IsOpened)
				return;

			// 090411 Use custom Descriptions mode
			if (Panel.Info.GetMode(PanelViewMode.AlternativeFull) == null)
			{
				PanelModeInfo mode = new PanelModeInfo();
				SetColumn c1 = new SetColumn(); c1.Name = "Format-Table"; c1.Type = "Z";
				mode.Columns = new FarColumn[] { c1 };
				Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode);
			}

			// base
			base.Show();
		}

		internal void TryFormat(object data)
		{
			object value = Cast<object>.From(data);
			if (value == null)
				return;

			TableControl table = A.FindTableControl(value.GetType().FullName, null);
			object[] newColumns;
			int count;
			if (table == null)
			{
				if (value is System.Collections.IEnumerable)
					return;

				List<Meta> metas = new List<Meta>();
				PSObject pso = PSObject.AsPSObject(data);
				foreach (PSPropertyInfo pi in pso.Properties)
				{
					// skip PS properties
					if (pi.Name.StartsWith("PS", StringComparison.Ordinal))
						continue;

					metas.Add(new Meta(pi.Name));
					if (metas.Count >= A.Psf.Settings.MaximumPanelColumnCount)
						break;
				}
				count = metas.Count;
				if (count == 0)
					return;
				newColumns = new object[count];
				for (int i = 0; i < count; ++i)
					newColumns[i] = metas[i];
			}
			else
			{
				count = Math.Min(table.Rows[0].Columns.Count, FarColumn.DefaultColumnTypes.Count);
				newColumns = new object[count];
				for (int i = 0; i < count; ++i)
					newColumns[i] = new Meta(table.Rows[0].Columns[i].DisplayEntry, table.Headers[i]);
			}

			// heuristic N
			if (count > 1 && SetBestType(newColumns, "N", Word.Name, "*" + Word.Name, Word.Id, Word.Key, "*" + Word.Key, "*" + Word.Id))
				--count;

			// heuristic Z
			if (count > 1 && SetBestType(newColumns, "Z", Word.Description, Word.Definition))
				--count;

			// heuristic O
			if (count > 1 && SetBestType(newColumns, "O", Word.Value, Word.Status))
				--count;

			Columns = newColumns;
		}

		static bool SetBestType(object[] columns, string type, params string[] patterns)
		{
			int bestRank = patterns.Length;
			Meta bestMeta = null;

			foreach (Meta meta in columns)
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

		/// <summary>
		/// Sets file name if any suitable exists.
		/// </summary>
		internal void SetFileName(FarFile file)
		{
			// case: meta name
			if (FarName != null)
			{
				file.Name = FarName.GetString(file.Data);
				return;
			}

			// case: try to get display name
			PSObject data = PSObject.AsPSObject(file.Data);
			PSPropertyInfo pi = A.FindDisplayProperty(data);
			if (pi != null)
			{
				file.Name = pi.Value == null ? "<null>" : pi.Value.ToString();
				return;
			}

			// other: use ToString(), but skip too verbose PSCustomObject
			if (!(data.BaseObject is PSCustomObject))
				file.Name = data.ToString();
		}

		void MakeMap()
		{
			// pass 1: get metas and types and pre-process only specified default types
			Meta[] metas = SetupColumns(Columns);

			// pass 2: process all, use still available default column types
			Map = new ObjectFileMap();
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
		/// Updates <see cref="FarFile.Data"/> and <see cref="FarFile.Name"/>
		/// and returns false or updates everything itself and returns true.
		/// </summary>
		internal abstract bool OnGettingData();

		/// <summary>
		/// Calls <see cref="OnGettingData()"/> and makes Description column.
		/// </summary>
		internal override sealed void OnGettingData(PanelEventArgs e)
		{
			FromGettingData = true;
			try
			{
				// call the worker
				bool done = OnGettingData();

				// empty?
				if (Panel.Files.Count == 0)
				{
					PanelModeInfo mode = Panel.Info.GetMode(PanelViewMode.AlternativeFull);
					if (mode.Columns.Length == 1 && mode.Columns[0].Name == "<empty>")
						return;
					
					// reuse: reset columns, keep other data current
					SetColumn c1 = new SetColumn();
					c1.Name = "<empty>";
					c1.Type = "N";
					mode.Columns = new FarColumn[] { c1 };
					Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode);
				}

				if (done)
					return;

				// 100202 use this always
				// 090927 try the first object for a linear type and, if it is, use only names
				foreach (FarFile file in Panel.Files)
					file.Description = file.Name;
			}
			finally
			{
				FromGettingData = false;
			}
		}

		//???? not used
		PanelModeInfo _EmptyMode_;
		PanelModeInfo EmptyMode
		{
			get
			{
				if (_EmptyMode_ == null)
				{
					_EmptyMode_ = new PanelModeInfo();
				}
				return _EmptyMode_;
			}
		}


		internal override string HelpMenuTextOpenFileMembers { get { return "Object members"; } }

	}
}
