/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Base class for table-like panels.
	/// </summary>
	public abstract class TablePanel : AnyPanel
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		internal TablePanel()
		{ }

		/// <summary>
		/// Members to exclude in a child <see cref="ListPanel"/>.
		/// </summary>
		public string[] ExcludeMembers { get; set; }

		/// <summary>
		/// Panel columns info. Set it only when the panel has no files yet.
		/// </summary>
		/// <remarks>
		/// See <see cref="Meta"/> remarks about use of dictionaries as items.
		/// <see cref="DataPanel"/> items should be mapped to properties only.
		/// For other panels items are similar to <c>Format-Table</c> parameter <c>-Property</c>.
		/// <para>
		/// All supported types: "N", "Z", "O", "S", "DC", "DM", "DA", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// Supported Far column type suffixes may be added to the end, e.g. NR, ST, DCB, and etc., see [Column types] in Far API.
		/// </para>
		/// <para>
		/// Default column type sequence: "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9".
		/// </para>
		/// <para>
		/// Type rules:
		/// <ul>
		/// <li>Specify column types only when you really have to do so, especially try to avoid C0..C9, let them to be processed by default.</li>
		/// <li>C0...C9 must be listed incrementally without gaps; but other types between them is OK. E.g. C0, C2 is bad; C0, N, C1 is OK.</li>
		/// <li>If a type is not specified then the next available from the remaining default sequence is taken.</li>
		/// <li>Column types cannot be specified more than once.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		public virtual object[] Columns
		{
			get { return _Columns; }
			set
			{
				if (Panel.Files.Count > 0)
					throw new InvalidOperationException("Panel must have no files.");

				_Columns = value;
			}
		}
		object[] _Columns;

		internal static readonly string[] DefaultColumnTypes = { "N", "Z", "O", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9" };

		/// <summary>
		/// Infrastructure. Gets meta objects for columns.
		/// </summary>
		/// <returns>Meta objects ready for column mapping.</returns>
		public static Meta[] SetupColumns(object[] columns)
		{
			Meta[] r = new Meta[columns.Length];
			List<string> availableColumnTypes = new List<string>(DefaultColumnTypes);

			// pass 1: pre-process specified default types, remove them from available
			int iCustom = 0;
			for (int iColumn = 0; iColumn < columns.Length; ++iColumn)
			{
				// meta data info
				Meta meta = new Meta(columns[iColumn]);
				r[iColumn] = meta;

				// skip not specified
				if (string.IsNullOrEmpty(meta.ColumnType))
					continue;

				// pre-process only default types: N, O, Z, C
				switch (meta.ColumnType[0])
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
							if (meta.ColumnType.Length < 2)
								throw new InvalidOperationException("Invalid column type: C");

							if (iCustom != (int)(meta.ColumnType[1] - '0'))
								throw new InvalidOperationException("Invalid column type: " + meta.ColumnType + ". Expected: C" + iCustom);

							availableColumnTypes.Remove(meta.ColumnType.Substring(0, 2));
							++iCustom;
						}
						break;
				}
			}

			// pass 2: set missed types from yet available
			int iAvailable = 0;
			foreach(Meta meta in r)
			{
				if (string.IsNullOrEmpty(meta.ColumnType))
				{
					if (iAvailable >= availableColumnTypes.Count)
						throw new InvalidOperationException("Too many columns.");

					meta.ColumnType = availableColumnTypes[iAvailable];
					++iAvailable;
				}
			}

			return r;
		}

		internal static PanelModeInfo SetupPanelMode(IList<Meta> metas)
		{
			PanelModeInfo r = new PanelModeInfo();
			r.ColumnTitles = new string[metas.Count];

			for (int i = 0; i < metas.Count; ++i)
			{
				r.ColumnTitles[i] = metas[i].ColumnTitle;
				if (i == 0)
				{
					r.ColumnTypes = metas[0].ColumnType;
					r.ColumnWidths = metas[0].ColumnWidth;
				}
				else
				{
					r.ColumnTypes += "," + metas[i].ColumnType;
					r.ColumnWidths += "," + metas[i].ColumnWidth;
				}
			}

			return r;
		}
		
		internal abstract string HelpMenuTextOpenFileMembers { get; }

		///
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			if (items.OpenFileMembers == null)
			{
				items.OpenFileMembers = new SetItem();
				items.OpenFileMembers.Text = HelpMenuTextOpenFileMembers;
				items.OpenFileMembers.Click = delegate { UIOpenFileMembers(); };
			}

			base.HelpMenuInitItems(items, e);
		}

	}
}
