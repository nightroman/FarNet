/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
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

		/// <include file='doc.xml' path='doc/Columns/*'/>
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

		///
		protected FormatPanel()
		{ }

		static Meta[] CutOffMetas(Meta[] metas)
		{
			if (metas.Length <= A.Psf.Settings.MaximumPanelColumnCount)
				return metas;

			Meta[] result = new Meta[A.Psf.Settings.MaximumPanelColumnCount];
			for (int i = result.Length; --i >= 0; )
				result[i] = metas[i];

			return result;
		}

		//! assume it is done for the active panel, it does not work well from the disk menu
		internal static Meta[] TryFormatByTableControl(PSObject value)
		{
			// try to find a table
			TableControl table = A.FindTableControl(value.BaseObject.GetType().FullName, null);
			if (table == null)
				return null;

			// convert all columns to meta
			Meta[] metas = new Meta[table.Rows[0].Columns.Count];
			for (int i = metas.Length; --i >= 0; )
				metas[i] = new Meta(table.Rows[0].Columns[i].DisplayEntry, table.Headers[i]);

			// 1) set heuristic types, some columns are moved to the left
			SetBestTypes(metas, A.Psf.Settings.MaximumPanelColumnCount);

			// 2) cut off too many columns
			metas = CutOffMetas(metas);

			// adjust formatting to the panel width
			int totalWidth = Far.Net.Panel.Window.Width - (metas.Length + 1); // N columns ~ N + 1 borders
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

			// fix too wide (less than 5 chars for unset columns), drop all positive widths
			if (setSum + (metas.Length - setCount) * 5 > totalWidth)
			{
				foreach (Meta meta in metas)
					if (meta.Width > 0)
						meta.Width = 0;
			}
			// fix too narrow (e.g. for Get-Service ~ 64), drop the maximum width
			else if (setCount == metas.Length && setSum < totalWidth)
			{
				metas[setMaxIndex].Width = 0;
			}

			return metas;
		}

		internal static Meta[] TryFormatByMembers(Collection<PSObject> values, bool single)
		{
			Meta[] metas;

			if (single)
			{
				var list = new List<Meta>();
				foreach (PSPropertyInfo pi in values[0].Properties)
					list.Add(new Meta(pi.Name));
				metas = list.ToArray();
			}
			else
			{
				Collection<PSObject> members = A.Psf.InvokeCode("$args[0] | Get-Member -MemberType Property -ErrorAction 0 | Select-Object -ExpandProperty Name", values);
				metas = new Meta[members.Count];
				for (int i = 0; i < members.Count; ++i)
					metas[i] = new Meta(members[i].ToString());
			}

			if (metas.Length == 0)
				return null;

			// 1) set heuristic types
			SetBestTypes(metas, A.Psf.Settings.MaximumPanelColumnCount);

			// 2) cut off
			return CutOffMetas(metas);
		}

		static void SetBestTypes(Meta[] metas, int maximum)
		{
			int count = metas.Length;

			// heuristic N
			if (count > 1 && SetBestType(metas, maximum, "N", Word.Name, "*" + Word.Name, Word.Id, Word.Key, "*" + Word.Key, "*" + Word.Id))
				--count;

			// heuristic Z
			if (count > 1 && SetBestType(metas, maximum, "Z", Word.Description, Word.Definition))
				--count;

			// heuristic O
			if (count > 1)
				SetBestType(metas, maximum, "O", Word.Value, Word.Status);
		}

		static bool SetBestType(Meta[] metas, int maximum, string type, params string[] patterns)
		{
			int iBestPattern = patterns.Length;
			int iBestMeta = -1;

			for (int iMeta = 0; iMeta < metas.Length; ++iMeta)
			{
				Meta meta = metas[iMeta];
				if (meta.Kind != null)
					continue;

				bool done = false;
				string name = meta.Name;
				for (int iPattern = 0; iPattern < iBestPattern; ++iPattern)
				{
					string pattern = patterns[iPattern];
					if (pattern[0] == '*')
					{
						if (name.EndsWith(patterns[iPattern].Substring(1), StringComparison.OrdinalIgnoreCase))
						{
							iBestMeta = iMeta;
							iBestPattern = iPattern;
						}
					}
					else
					{
						if (string.Compare(name, patterns[iPattern], StringComparison.OrdinalIgnoreCase) == 0)
						{
							iBestMeta = iMeta;
							if (iPattern == 0)
							{
								done = true;
								break;
							}

							iBestPattern = iPattern;
						}
					}
				}

				if (done)
					break;
			}

			// no candidates
			if (iBestMeta < 0)
				return false;

			// set the column type
			metas[iBestMeta].Kind = type;

			// done for small column set
			if (metas.Length <= maximum)
				return true;

			// move the best to the first free position
			for (int iFree = 0; iFree < maximum; ++iFree)
			{
				if (metas[iFree].Kind == null)
				{
					if (iFree != iBestMeta)
					{
						Meta meta = metas[iBestMeta];
						for (int iMove = iBestMeta; iMove > iFree; --iMove)
							metas[iMove] = metas[iMove - 1];
						metas[iFree] = meta;
					}
					break;
				}
			}

			return true;
		}

		void MakeMap(Meta[] metas) //???
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
				switch (meta.Kind[0])
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
							if (meta.Kind.Length < 2)
								throw new InvalidOperationException(Res.InvalidColumnKind + "D");

							switch (meta.Kind[1])
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
									throw new InvalidOperationException(Res.InvalidColumnKind + meta.Kind);
							}
						}
						break;
					default:
						throw new InvalidOperationException("Unknown column type: " + meta.Kind);
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
					Far.Net.ShowError(Res.Me, ex);

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
				mode.Columns = new FarColumn[] { new SetColumn() { Kind = "N", Name = "<empty>" } };
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

				// Check some special cases and try to get the common type.
				// ???? _100309_121508 Linear type case
				Type theType;
				if (Converter.IsLinearType(values[0].BaseObject.GetType()) ||
					values[0].BaseObject is System.Collections.IEnumerable ||
					null == (theType = A.FindCommonType(values)))
				{
					// use index, value, type mode
					BuildFilesMixed(values);
					return;
				}

				Meta[] metas = null;

				// try to get format
				if (theType != typeof(PSCustomObject))
					metas = TryFormatByTableControl(values[0]);

				// use members
				if (metas == null)
					metas = TryFormatByMembers(values, theType != null && theType == values[0].BaseObject.GetType());

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
			var files = new List<FarFile>(values.Count);
			Panel.Files = files;

			foreach (PSObject value in values)
				files.Add(new MapFile(value, Map));
		}

		void BuildFilesMixed(Collection<PSObject> values)
		{
			var files = new List<FarFile>(values.Count);
			Panel.Files = files;

			PanelModeInfo mode = Panel.Info.GetMode(PanelViewMode.AlternativeFull);
			if (mode == null)
				mode = new PanelModeInfo();
			mode.Columns = new FarColumn[]
			{
				new SetColumn() { Kind = "S", Name = "##"}, // "Index" clashes to sort order mark
				new SetColumn() { Kind = "N", Name = "Value"},
				new SetColumn() { Kind = "Z", Name = "Type"}
			};
			Panel.Info.SetMode(PanelViewMode.AlternativeFull, mode);

			int index = 0;
			foreach (PSObject value in values)
			{
				// new file
				SetFile file = new SetFile()
				{
					Data = value,
					Length = index++,
					Description = value.BaseObject.GetType().FullName
				};

				// discover name
				// ???? _100309_121508 Linear type case
				IEnumerable asIEnumerable;
				PSPropertyInfo pi;
				if (Converter.IsLinearType(value.BaseObject.GetType()))
					file.Name = value.ToString();
				else if ((asIEnumerable = value.BaseObject as IEnumerable) != null)
					file.Name = Converter.FormatEnumerable(asIEnumerable, A.Psf.Settings.FormatEnumerationLimit);
				else if ((pi = A.FindDisplayProperty(value)) != null)
					file.Name = pi.Value.ToString();
				else
					file.Name = value.ToString();

				// add
				files.Add(file);
			}
		}

		internal override string HelpMenuTextOpenFileMembers { get { return "Object members"; } }

	}
}
