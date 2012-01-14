
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	static class Format
	{
		static Meta[] CutOffMetas(Meta[] metas)
		{
			if (metas.Length <= Settings.Default.MaximumPanelColumnCount)
				return metas;

			Meta[] result = new Meta[Settings.Default.MaximumPanelColumnCount];
			for (int i = result.Length; --i >= 0; )
				result[i] = metas[i];

			return result;
		}
		//! assume it is done for the active panel, it does not work well from the disk menu
		internal static Meta[] TryFormatByTableControl(PSObject value, int formatWidth)
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
			SetBestTypes(metas, Settings.Default.MaximumPanelColumnCount);

			// 2) cut off too many columns
			metas = CutOffMetas(metas);

			// adjust formatting to the panel width
			int totalWidth = formatWidth - (metas.Length + 1); // N columns ~ N + 1 borders
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
		internal static Meta[] TryFormatByMembers(Collection<PSObject> values, bool homogeneous)
		{
			Meta[] metas;

			//! homogeneous: use the first sample and the _100426_034702
			if (homogeneous)
			{
				PSObject value = values[0];
				var membersToShow = new List<string>();
				{
					string code = "Get-Member -InputObject $args[0] -MemberType Properties -ErrorAction 0";
					foreach (PSObject o in A.InvokeCode(code, value))
						membersToShow.Add(o.Properties[Word.Name].Value.ToString());
				}
				var list = new List<Meta>(membersToShow.Count);
				foreach (PSPropertyInfo pi in values[0].Properties)
					if (membersToShow.Contains(pi.Name))
						list.Add(new Meta(pi.Name));
				metas = list.ToArray();
			}
			// heterogeneous: just get mixed properties as they are
			else
			{
				var members = A.InvokeCode("$args[0] | Get-Member -MemberType Properties -ErrorAction 0", values);
				metas = new Meta[members.Count];
				for (int i = 0; i < members.Count; ++i)
					metas[i] = new Meta(members[i].Properties[Word.Name].Value.ToString());
			}

			if (metas.Length == 0)
				return null;

			// 1) set heuristic types
			SetBestTypes(metas, Settings.Default.MaximumPanelColumnCount);

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
						if (string.Equals(name, patterns[iPattern], StringComparison.OrdinalIgnoreCase))
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
			int end = Math.Min(maximum, iBestMeta);
			for (int iFree = 0; iFree < end; ++iFree)
			{
				if (metas[iFree].Kind == null)
				{
					Meta meta = metas[iBestMeta];
					for (int iMove = iBestMeta; iMove > iFree; --iMove)
						metas[iMove] = metas[iMove - 1];
					metas[iFree] = meta;
					break;
				}
			}

			return true;
		}
		// _101125_173951 Out-GridView approach:
		// show Index (##), Value, Type columns for mixed types and the only 'TypeName' column for the same type.
		internal static string BuildFilesMixed(IList<FarFile> files, Collection<PSObject> values)
		{
			files.Clear();

			int index = -1;
			string sameType = null;
			foreach (PSObject value in values)
			{
				++index;

				// new file
				SetFile file = new SetFile() { Data = value, Length = index };

				// description: watch the same type to choose the panel columns and to reuse same type string
				string newType = value.BaseObject.GetType().FullName;
				if (index == 0)
				{
					sameType = newType;
					file.Description = newType;
				}
				else if (sameType == null)
				{
					file.Description = newType;
				}
				else if (sameType == newType)
				{
					// use the same reference
					file.Description = sameType;
				}
				else
				{
					sameType = null;
					file.Description = newType;
				}

				// discover name
				// _100309_121508 Linear type case
				IEnumerable asIEnumerable;
				PSPropertyInfo pi;
				if (Converter.IsLinearType(value.BaseObject.GetType()))
					file.Name = value.ToString();
				else if ((asIEnumerable = value.BaseObject as IEnumerable) != null)
					file.Name = Converter.FormatEnumerable(asIEnumerable, Settings.Default.FormatEnumerationLimit);
				else if ((pi = A.FindDisplayProperty(value)) != null)
					file.Name = (pi.Value ?? string.Empty).ToString();
				else
					file.Name = value.ToString();

				// add
				files.Add(file);
			}

			return sameType;
		}
		internal static FileMap MakeMap(ref Meta[] metas, object[] columns)
		{
			// pass 1: get metas and types and pre-process only specified default types
			if (metas == null)
				metas = SetupColumns(columns);
			else
				SetupMetas(metas);

			// pass 2: process all, use still available default column types
			var map = new FileMap();
			foreach (Meta meta in metas)
			{
				// type -> map:
				switch (meta.Kind[0])
				{
					case 'N':
						map.Name = meta;
						break;
					case 'O':
						map.Owner = meta;
						break;
					case 'Z':
						map.Description = meta;
						break;
					case 'C':
						map.Columns.Add(meta);
						break;
					case 'S':
						{
							if (map.Length != null)
								throw new InvalidOperationException("Column 'S' is used twice.");
							map.Length = meta;
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
										if (map.CreationTime != null)
											throw new InvalidOperationException("Column 'DC' is used twice.");

										map.CreationTime = meta;
									}
									break;
								case 'M':
									{
										if (map.LastWriteTime != null)
											throw new InvalidOperationException("Column 'DM' is used twice.");

										map.LastWriteTime = meta;
									}
									break;
								case 'A':
									{
										if (map.LastAccessTime != null)
											throw new InvalidOperationException("Column 'DA' is used twice.");

										map.LastAccessTime = meta;
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

			return map;
		}
		internal static Meta[] SetupColumns(object[] columns)
		{
			Meta[] metas = new Meta[columns.Length];
			for (int iColumn = 0; iColumn < columns.Length; ++iColumn)
				metas[iColumn] = Meta.AsMeta(columns[iColumn]);

			SetupMetas(metas);
			return metas;
		}
		internal static void SetupMetas(Meta[] metas)
		{
			var availableColumnTypes = new List<string>(FarColumn.DefaultColumnKinds);

			// pass 1: pre-process specified default types, remove them from available
			int iCustom = 0;
			for (int iColumn = 0; iColumn < metas.Length; ++iColumn)
			{
				// meta data info
				Meta meta = metas[iColumn];

				// skip not specified
				if (string.IsNullOrEmpty(meta.Kind))
					continue;

				// pre-process only default types: N, Z, O, C
				switch (meta.Kind[0])
				{
					case 'N':
						{
							if (!availableColumnTypes.Remove("N"))
								throw new InvalidOperationException("Column 'N' is used twice.");
						}
						break;
					case 'O':
						{
							if (!availableColumnTypes.Remove("O"))
								throw new InvalidOperationException("Column 'O' is used twice.");
						}
						break;
					case 'Z':
						{
							if (!availableColumnTypes.Remove("Z"))
								throw new InvalidOperationException("Column 'Z' is used twice.");
						}
						break;
					case 'C':
						{
							if (meta.Kind.Length < 2)
								throw new InvalidOperationException(Res.InvalidColumnKind + "C");

							if (iCustom != (int)(meta.Kind[1] - '0'))
								throw new InvalidOperationException(Res.InvalidColumnKind + meta.Kind + ". Expected: C" + iCustom);

							availableColumnTypes.Remove(meta.Kind.Substring(0, 2));
							++iCustom;
						}
						break;
				}
			}

			// pass 2: set missed types from yet available
			int iAvailable = 0;
			foreach (Meta meta in metas)
			{
				if (string.IsNullOrEmpty(meta.Kind))
				{
					if (iAvailable >= availableColumnTypes.Count)
						throw new InvalidOperationException("Too many columns.");

					meta.Kind = availableColumnTypes[iAvailable];
					++iAvailable;
				}
			}
		}
		/// <summary>
		/// Gets meta objects for columns.
		/// </summary>
		/// <returns>Meta objects ready for column mapping.</returns>
		internal static PanelPlan SetupPanelMode(IList<Meta> metas)
		{
			PanelPlan r = new PanelPlan();

			r.Columns = new FarColumn[metas.Count];
			for (int i = 0; i < metas.Count; ++i)
				r.Columns[i] = metas[i];

			return r;
		}
	}
}
