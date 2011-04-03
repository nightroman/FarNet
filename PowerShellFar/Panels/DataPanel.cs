
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Panel exploring a data table.
	/// </summary>
	public sealed class DataPanel : TablePanel, IDisposable
	{
		///
		new DataExplorer Explorer { get { return (DataExplorer)base.Explorer; } }
		/// <summary>
		/// Constructor.
		/// </summary>
		public DataPanel()
			: base(new DataExplorer())
		{
			Explorer.Panel = this;

			CurrentLocation = "*";
			SortMode = PanelSortMode.Unsorted; // assume it is sorted in SELECT
			UseFilter = true;
			UseSortGroups = false;
		}
		/// <summary>
		/// Database provider factory instance.
		/// See <b>System.Data.Common.DbProviderFactories</b> methods <b>GetFactoryClasses</b>, <b>GetFactory</b>.
		/// </summary>
		public DbProviderFactory Factory { get; set; }
		/// <summary>
		/// Data adapter.
		/// You have to set it and configure at least its <c>SelectCommand</c>.
		/// </summary>
		public DbDataAdapter Adapter { get; set; }
		/// <summary>
		/// A table which records are used as panel items.
		/// </summary>
		/// <remarks>
		/// Normally this table is created, assigned and filled internally.
		/// An external table is possible but this scenario is mostly reserved for the future.
		/// </remarks>
		public DataTable Table { get; set; }
		/// <summary>
		/// Connection.
		/// </summary>
		public DbConnection Connection
		{
			get { return Adapter == null || Adapter.SelectCommand == null ? null : Adapter.SelectCommand.Connection; }
		}
		readonly DataRowFileMap Map = new DataRowFileMap();
		/// <summary>
		/// Disposes internal data. Normally it is called internally.
		/// </summary>
		public void Dispose()
		{
			if (_Builder != null)
				_Builder.Dispose();

			if (Table != null)
				Table.Dispose();
		}
		/// <summary>
		/// Calls <c>Adapter.Update()</c>.
		/// </summary>
		public override bool SaveData()
		{
			// Build commands
			BuildDeleteCommand();
			BuildInsertCommand();
			BuildUpdateCommand();

			try
			{
				ToUpdateData = true;
				Adapter.Update(Table);
				return true;
			}
			catch (DBConcurrencyException ex)
			{
				A.Msg(ex);
			}
			catch (DbException ex)
			{
				A.Msg(ex);
			}
			return false;
		}
		///
		protected override string DefaultTitle { get { return string.IsNullOrEmpty(Table.TableName) ? "Data Table" : "Table " + Table.TableName; } }
		/// <summary>
		/// Fills data table and shows the panel.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override void Open()
		{
			if (IsOpened)
				return;

			if (Table == null && Adapter == null)
				throw new RuntimeException("The table and the adapter are null.");
			if (Table == null && Adapter.SelectCommand == null)
				throw new RuntimeException("The table and the adapter select command are null.");

			// fill table
			if (Table == null)
			{
				Table = new DataTable();
				Table.Locale = CultureInfo.CurrentCulture; // CA
				Fill();
			}

			// pass 1: collect the columns
			IList<Meta> metas;
			if (Columns == null)
			{
				// collect all table columns skipping not linear data types
				int Count = Math.Min(Table.Columns.Count, A.Psf.Settings.MaximumPanelColumnCount);
				metas = new List<Meta>(Count);
				int nCollected = 0;
				foreach (DataColumn column in Table.Columns)
				{
					if (Converter.IsLinearType(column.DataType))
					{
						Meta meta = new Meta(column.ColumnName);
						meta.Kind = FarColumn.DefaultColumnKinds[nCollected];
						metas.Add(meta);
						++nCollected;
						if (nCollected >= Count)
							break;
					}
				}
			}
			else
			{
				// setup user defined columns
				metas = Format.SetupColumns(Columns);
			}

			// at least one column
			if (metas.Count == 0)
				throw new InvalidOperationException("There is no column to display.");

			// pass 2: mapping
			foreach (Meta meta in metas)
			{
				DataColumn column = Table.Columns[meta.Property];

				switch (meta.Kind[0])
				{
					case 'N':
						Map.Name = column.Ordinal;
						break;
					case 'O':
						Map.Owner = column.Ordinal;
						break;
					case 'Z':
						Map.Description = column.Ordinal;
						break;
					case 'C':
						Map.Columns.Add(column.Ordinal);
						break;
					case 'S':
						{
							if (Map.Length >= 0)
								throw new InvalidOperationException("Column 'S' is used twice.");
							Map.Length = column.Ordinal;
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
										if (Map.CreationTime >= 0)
											throw new InvalidOperationException("Column 'DC' is used twice.");

										Map.CreationTime = column.Ordinal;
									}
									break;
								case 'M':
									{
										if (Map.LastWriteTime >= 0)
											throw new InvalidOperationException("Column 'DM' is used twice.");

										Map.LastWriteTime = column.Ordinal;
									}
									break;
								case 'A':
									{
										if (Map.LastAccessTime >= 0)
											throw new InvalidOperationException("Column 'DA' is used twice.");

										Map.LastAccessTime = column.Ordinal;
									}
									break;
								default:
									throw new InvalidOperationException(Res.InvalidColumnKind + meta.Kind);
							}
						}
						break;
					default:
						throw new InvalidOperationException(Res.InvalidColumnKind + meta.Kind);
				}
			}

			// pass 3: set plan
			SetPlan(PanelViewMode.AlternativeFull, Format.SetupPanelMode(metas));

			base.Open();
		}
		///??
		protected override bool CanClose()
		{
			using (DataTable dt = Table.GetChanges())
			{
				if (dt == null)
					return true;
			}

			switch (Far.Net.Message(Res.AskSaveModified, "Save", MsgOptions.YesNoCancel))
			{
				case 0:
					return SaveData();
				case 1:
					Table.RejectChanges(); // to avoid request
					return true;
				default:
					return false;
			}
		}
		internal void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			var Files = Explorer.Cache;
			BuildDeleteCommand();

			if (args.UI && 0 != (Far.Net.Confirmations & FarConfirmations.Delete))
			{
				if (0 != Far.Net.Message("Delete selected record(s)?", Res.Delete, MsgOptions.None, new string[] { Res.Delete, Res.Cancel }))
					return;
			}

			ToUpdateData = true;

			foreach (var file in args.Files)
			{
				var dr = file.Data as DataRow;
				if (dr == null)
				{
					Files.Remove(file);
					continue;
				}

				bool ok = true;
				try
				{
					dr.Delete();
					ok = SaveData();
				}
				catch
				{
					ok = false;
					throw;
				}
				finally
				{
					if (!ok)
					{
						dr.RejectChanges();
						PostData(dr);
					}
				}
				if (!ok)
					break;
			}
		}
		bool __toUpdateData = true;
		bool ToUpdateData
		{
			get { return __toUpdateData; }
			set { __toUpdateData = value; }
		}
		internal IList<FarFile> Explore(GetFilesEventArgs args)
		{
			if (args == null) return null;
			var Files = Explorer.Cache;

			// refill
			if (UserWants == UserAction.CtrlR && Adapter != null)
			{
				if (CanClose())
				{
					Table.Clear();
					Fill();
				}
			}

			// no job?
			if (!ToUpdateData && UserWants != UserAction.CtrlR)
				return Files;

			// refresh data
			for (int iFile = Files.Count; --iFile >= 0; )
			{
				FarFile f = Files[iFile];
				DataRow Row = f.Data as DataRow;
				if (Row == null || Row.RowState == DataRowState.Deleted || Row.RowState == DataRowState.Detached)
				{
					Files.RemoveAt(iFile);
					continue;
				}
			}

			// prevent next job
			ToUpdateData = false;
			return Files;
		}
		/// <summary>
		/// Opens a member panel to edit the record.
		/// </summary>
		public override void OpenFile(FarFile file)
		{
			var memberPanel = OpenFileMembers(file);
			memberPanel.Explorer.CanDeleteFiles = false;
		}
		int RecordLimit = A.Psf.Settings.MaximumPanelFileCount;
		// 0-based internally and in UI: 'offset' means "skip 'offset' records"
		int RecordOffset;
		void Fill()
		{
			Adapter.Fill(RecordOffset, RecordLimit, Table);

			var Files = Explorer.Cache;
			Files.Clear();
			foreach (DataRow dr in Table.Rows)
				Files.Add(new DataRowFile(dr, Map));
		}
		internal override void ShowHelp()
		{
			Help.ShowTopic("DataPanel");
		}
		// Command builder
		DbCommandBuilder _Builder;
		void EnsureBuilder()
		{
			if (_Builder != null)
				return;

			if (Factory == null)
				throw new RuntimeException("Cannot create a command builder because this.Factory is null.");

			if (Adapter == null)
				Adapter = Factory.CreateDataAdapter();

			_Builder = Factory.CreateCommandBuilder();
			_Builder.DataAdapter = Adapter;
		}
		/// <summary>
		/// Builds DELETE command.
		/// </summary>
		public void BuildDeleteCommand()
		{
			if (Adapter != null && Adapter.DeleteCommand != null)
				return;

			EnsureBuilder();
			Adapter.DeleteCommand = _Builder.GetDeleteCommand();
		}
		/// <summary>
		/// Builds INSERT command.
		/// </summary>
		public void BuildInsertCommand()
		{
			if (Adapter != null && Adapter.InsertCommand != null)
				return;

			EnsureBuilder();
			Adapter.InsertCommand = _Builder.GetInsertCommand();
		}
		/// <summary>
		/// Builds UPDATE command.
		/// </summary>
		public void BuildUpdateCommand()
		{
			if (Adapter != null && Adapter.UpdateCommand != null)
				return;

			EnsureBuilder();
			Adapter.UpdateCommand = _Builder.GetUpdateCommand();
		}
		void OnRangeNext()
		{
			if (Table.Rows.Count < RecordLimit)
				return;

			RecordOffset += RecordLimit;

			UserWants = UserAction.CtrlR;
			UpdateRedraw(true);
		}
		void OnRangePrevious()
		{
			if (RecordOffset == 0)
				return;

			RecordOffset -= RecordLimit;
			if (RecordOffset < 0)
				RecordOffset = 0;

			UserWants = UserAction.CtrlR;
			UpdateRedraw(true);
		}
		///
		public override bool UIKeyPressed(int code, KeyStates state) //????? docs
		{
			switch (code)
			{
				case VKeyCode.PageDown:
					switch (state)
					{
						case KeyStates.Control | KeyStates.Shift:
							OnRangeNext();
							return true;
					}
					break;
				case VKeyCode.PageUp:
					switch (state)
					{
						case KeyStates.Control | KeyStates.Shift:
							OnRangePrevious();
							return true;
					}
					break;
			}
			return base.UIKeyPressed(code, state);
		}
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			e.Menu.Add("Next range").Click = delegate { OnRangeNext(); };

			e.Menu.Add("Previous range").Click = delegate { OnRangePrevious(); };

			e.Menu.Add("Record limit").Click = delegate
			{
				var text = Far.Net.Input("Record limit", "DataRecordLimit", "Data Panel", RecordLimit.ToString());
				if (string.IsNullOrEmpty(text))
					return;

				int value;
				if (!int.TryParse(text, out value) || value < 1)
				{
					A.Message("Invalid number");
					return;
				}

				RecordLimit = value;

				UserWants = UserAction.CtrlR;
				UpdateRedraw(true);
			};

			e.Menu.Add("Record offset").Click = delegate
			{
				var text = Far.Net.Input("Record offset", "DataRecordOffset", "Data Panel", RecordOffset.ToString());
				if (string.IsNullOrEmpty(text))
					return;

				int value;
				if (!int.TryParse(text, out value) || value < 0)
				{
					A.Message("Invalid number");
					return;
				}

				RecordOffset = value;

				UserWants = UserAction.CtrlR;
				UpdateRedraw(true);
			};

			e.Menu.Add(string.Empty).IsSeparator = true;

			if (items.Create == null)
				items.Create = new SetItem()
				{
					Text = "&New row",
					Click = delegate { UICreate(); }
				};

			if (items.Delete == null)
				items.Delete = new SetItem()
				{
					Text = "&Delete row(s)",
					Click = delegate { UIDelete(false); }
				};

			base.HelpMenuInitItems(items, e);
		}
		internal override string HelpMenuTextOpenFileMembers { get { return "Edit row data"; } }
		internal void DoCreateFile()
		{
			BuildInsertCommand();

			// add new row to the table
			DataRow dr = Table.NewRow();
			Table.Rows.Add(dr);

			// new dummy file, do not add, it can be canceled
			DataRowFile file = new DataRowFile(dr, Map); //_110330_175246

			// open the record panel
			OpenFile(file);
		}
		///
		protected override bool CanCloseChild()
		{
			MemberPanel mp = Child as MemberPanel;
			if (mp == null)
				return true;

			DataRow dr = mp.Value.BaseObject as DataRow;
			if (dr == null)
				return true;

			var xRowState = dr.RowState;
			if (0 == (xRowState & (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified)))
				return true;

			switch (Far.Net.Message(Res.AskSaveModified, "Save", MsgOptions.YesNoCancel))
			{
				case 0:
					// save data, update the table
					var result = SaveData();

					// now we can add a new file for the new record
					if (xRowState == DataRowState.Added) //_110330_175246
					{
						DataRowFile file = new DataRowFile(dr, Map);
						Explorer.Cache.Add(file);
						PostFile(file);
						ToUpdateData = true;
					}

					return result;

				case 1:
					dr.RejectChanges();
					return true;

				default:
					return false;
			}
		}
		///
		public override void UIClosed()
		{
			try
			{
				if (Adapter != null)
				{
					using (var dt = Table.GetChanges())
					{
						if (dt != null && Far.Net.Message(Res.AskSaveModified, "Save", MsgOptions.YesNo) == 0)
							SaveData();
					}
				}

				Dispose();
			}
			finally
			{
				base.UIClosed();
			}
		}
	}
}
