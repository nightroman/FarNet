/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
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
		///
		internal TablePanel()
		{ }

		/// <include file='doc.xml' path='docs/pp[@name="Columns"]/*'/>
		public virtual object[] Columns
		{
			get { return _Columns; }
			set
			{
				if (Panel.Files.Count > 0)
					throw new InvalidOperationException("Panel must have no files for setting columns.");

				_Columns = value;
			}
		}
		object[] _Columns;

		/// <summary>
		/// Members to exclude in a child <see cref="ListPanel"/>.
		/// </summary>
		public string[] ExcludeMembers { get; set; }

		/// <summary>
		/// For internal use. Gets meta objects for columns.
		/// </summary>
		/// <returns>Meta objects ready for column mapping.</returns>
		public static Meta[] SetupColumns(object[] columns) //???? public
		{
			Meta[] metas = new Meta[columns.Length];
			for (int iColumn = 0; iColumn < columns.Length; ++iColumn)
				metas[iColumn] = Meta.AsMeta(columns[iColumn]);

			SetupMetas(metas);
			return metas;
		}

		internal static void SetupMetas(Meta[] metas)
		{
			List<string> availableColumnTypes = new List<string>(FarColumn.DefaultColumnKinds);

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

		internal static PanelModeInfo SetupPanelMode(IList<Meta> metas)
		{
			PanelModeInfo r = new PanelModeInfo();

			r.Columns = new FarColumn[metas.Count];
			for (int i = 0; i < metas.Count; ++i)
				r.Columns[i] = metas[i];

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
