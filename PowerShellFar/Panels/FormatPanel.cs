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
	/// Abstract panel with formatting mode.
	/// </summary>
	public abstract class FormatPanel : TablePanel
	{
		// internal so far ???
		internal ObjectFileMap Map;

		// internal ???
		internal bool IsGettingData { get { return _IsGettingData; } }
		bool _IsGettingData;

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
			
			foreach(Meta meta in columns)
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
			_IsGettingData = true;
			try
			{
				// call the worker
				if (OnGettingData())
					return;

				// collect data
				IList<object> collectedData = CollectData();

				// empty?
				if (collectedData.Count == 0)
				{
					Panel.DotsDescription = "<empty>";
					return;
				}

				// 090927 try the first object for a linear type and, if it is, use only names
				PSObject sample = PSObject.AsPSObject(collectedData[0]);
				if (Converter.IsLinearType(sample.BaseObject.GetType()))
				{
					Panel.DotsDescription = "..";
					foreach (FarFile file in Panel.Files)
						file.Description = file.Name;
					return;
				}

				Panel.DotsDescription = null;
				using (PowerShell p = A.Psf.CreatePipeline())
				{
					Command c;

					string[] prmProperty = null;

					// Select-Object -Property -ExcludeProperty
					if (ExcludeColumns != null)
					{
						//! use -Property * if include is not set, e.g. cert: provider will not exclude PS*
						// 090823 BUT it makes problems for alias: provider: ls alias: | select *
						// I do not use -ea 0, let's it fail and users removes -ExcludeColumns.
						// Better workaround: to filter Get-Member on my own.
#if true
						Collection<PSObject> members = A.Psf.InvokeCode(@"
$args[0] |
Get-Member -MemberType Properties |
.{process{ $_.Name }} |
Select-Object -Unique
", collectedData);
						List<string> propertyToFilter = new List<string>(members.Count);
						foreach (PSObject o in members)
							propertyToFilter.Add(o.ToString());

						foreach (string pattern in ExcludeColumns)
						{
							WildcardPattern wp = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
							for (int i = propertyToFilter.Count; --i >= 0; )
							{
								if (wp.IsMatch(propertyToFilter[i]))
									propertyToFilter.RemoveAt(i);
							}
						}

						prmProperty = new string[propertyToFilter.Count];
						propertyToFilter.CopyTo(prmProperty);

#else
						c = new Command("Select-Object");
						c.Parameters.Add("Property", "*");
						c.Parameters.Add("ExcludeProperty", ExcludeColumns);
						p.Commands.Add(c);
#endif
					}

					// formatting width
					int width = Panel.Window.Width - 2;

					// Format-Table
					c = new Command("Format-Table");
					if (prmProperty != null)
						c.Parameters.Add("Property", prmProperty);
					//! -AutoSize is slow, but otherwise lines can be wrapped even without -Wrap: e.g. try 'ls'.
					//! Fortunately (and funny) it looks like -Wrap:$false works fine.
					if (AutoSize)
						c.Parameters.Add("AutoSize", true);
					else
						c.Parameters.Add("Wrap", false);
					c.Parameters.Add(Prm.EASilentlyContinue);
					//! Trick: false grouping avoids unwanted groups, e.g. (ls -r) - groups by directory
					c.Parameters.Add("GroupBy", " ");
					p.Commands.AddCommand(c);

					// Out-String
					c = new Command("Out-String");
					c.Parameters.Add("Stream");
					c.Parameters.Add("Width", width);
					p.Commands.AddCommand(c);

					// invoke with current(!) data
					Collection<PSObject> desc = p.Invoke(collectedData);
					if (!A.ShowError(p))
						FormatTableToDescriptions(desc);
				}
			}
			catch (RuntimeException exception)
			{
				if ((e.Mode & OperationModes.FindSilent) == 0)
					A.Msg(exception.Message);
			}
			finally
			{
				_IsGettingData = false;
			}
		}

		/// <summary>
		/// Converts Format-Table output to Descriptions.
		/// </summary>
		bool FormatTableToDescriptionsWorker(Collection<PSObject> descriptions)
		{
			// skip empty head lines
			int i = 0;
			while (i < descriptions.Count && descriptions[i].ToString().Length == 0)
			{ ++i; }

			// wrong line count?
			int n = descriptions.Count;
			if (n - i - 2 < Panel.Files.Count)
				return false;

			// tail lines must be empty
			for (int j = i + 2 + Panel.Files.Count; j < n; ++j)
				if (descriptions[j].ToString().Length != 0)
					return false;

			// [1] must be table separator
			if (!Kit.RegexTableSeparator.IsMatch(descriptions[i + 1].ToString()))
				return false;

			// [0] is the header
			Panel.DotsDescription = descriptions[i].ToString();

			// rows
			i += 2;
			foreach (FarFile file in Panel.Files)
			{
				file.Description = descriptions[i].ToString();
				++i;
			}

			return true;
		}

		/// <summary>
		/// Converts Format-Table output to Descriptions or just uses Name on problems.
		/// </summary>
		void FormatTableToDescriptions(Collection<PSObject> descriptions)
		{
			// try to use formatted data
			if (FormatTableToDescriptionsWorker(descriptions))
				return;

			// we could not, show warning
			if (AutoSize)
				Panel.DotsDescription = "WARNING: Cannot format. Use -Columns, wider panel, similar objects.";
			else
				Panel.DotsDescription = "WARNING: Cannot format. Use -AutoSize, -Columns, wider panel, similar objects.";

			// use just names
			foreach (FarFile file in Panel.Files)
				file.Description = file.Name;
		}

		internal override string HelpMenuTextOpenFileMembers { get { return "Object members"; } }

	}
}
