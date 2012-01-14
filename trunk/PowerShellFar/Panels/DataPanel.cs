
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
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
			UseSortGroups = false;
		}
		DateTime _XmlFileTime;
		/// <summary>
		/// Gets or sets the XML data source file.
		/// </summary>
		public string XmlFile
		{
			get { return _XmlFile; }
			set { _XmlFile = string.IsNullOrEmpty(value) ? null : Path.GetFullPath(value); }
		}
		string _XmlFile;
		/// <summary>
		/// Gets or sets the XML schema definition file.
		/// </summary>
		public string XmlSchema
		{
			get { return _XmlSchema; }
			set { _XmlSchema = string.IsNullOrEmpty(value) ? null : Path.GetFullPath(value); }
		}
		string _XmlSchema;
		/// <summary>
		/// Gets or sets the table name.
		/// </summary>
		public string TableName { get; set; }
		/// <summary>
		/// Gets or sets the XML reading mode. 
		/// </summary>
		public XmlReadMode XmlReadMode { get; set; }
		/// <summary>
		/// Gets or sets the XML writing mode. 
		/// </summary>
		public XmlWriteMode XmlWriteMode { get; set; }
		static DataTable GetTable(DataSet dataSet, string tableName)
		{
			if (dataSet.Tables.Count == 0)
				throw new InvalidOperationException("Empty data set.");

			if (!string.IsNullOrEmpty(tableName))
				return dataSet.Tables[tableName];

			if (dataSet.Tables.Count == 1)
				return dataSet.Tables[0];

			var menu = Far.Net.CreateListMenu();
			menu.Title = "Tables";
			menu.UsualMargins = true;

			foreach (DataTable table in dataSet.Tables)
				menu.Add(table.TableName);

			int index = menu.Show() ? menu.Selected : 0;
			return dataSet.Tables[index];
		}
		/// <summary>
		/// Database provider factory instance.
		/// See <b>System.Data.Common.DbProviderFactories</b> methods <b>GetFactoryClasses</b>, <b>GetFactory</b>.
		/// </summary>
		public DbProviderFactory Factory { get; set; }
		/// <summary>
		/// Connection.
		/// </summary>
		public DbConnection Connection
		{
			get { return Adapter == null || Adapter.SelectCommand == null ? null : Adapter.SelectCommand.Connection; }
		}
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
		/// If an external table is set then <see cref="XmlFile"/> or <see cref="AsSaveData"/> might be useful.
		/// </remarks>
		public DataTable Table { get; set; }
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
		/// <include file='doc.xml' path='doc/ScriptFork/*'/>
		public sealed override bool SaveData()
		{
			if (AsSaveData != null)
				return LanguagePrimitives.IsTrue(A.InvokeScriptReturnAsIs(AsSaveData, this, null));
			else
				return DoSaveData();
		}
		/// <summary>
		/// Called by <see cref="SaveData"/>.
		/// </summary>
		public ScriptBlock AsSaveData { get; set; }
		/// <summary>
		/// Called by <see cref="SaveData"/>.
		/// </summary>
		public bool DoSaveData()
		{
			try
			{
				ToUpdateData = true;

				if (Adapter != null)
				{
					Adapter.Update(Table);
				}
				else if (!string.IsNullOrEmpty(_XmlFile))
				{
					// conflict?
					if (_XmlFileTime != DateTime.MinValue && File.Exists(_XmlFile) && _XmlFileTime != File.GetLastWriteTime(_XmlFile))
					{
						Far.Net.Message("Cannot save because the source file is modified.", "Conflict");
						return false;
					}

					// write
					if (Table.DataSet == null) //! user Table.DataSet can be null
						Table.WriteXml(_XmlFile, XmlWriteMode);
					else
						Table.DataSet.WriteXml(_XmlFile, XmlWriteMode);

					// stamp
					_XmlFileTime = File.GetLastWriteTime(_XmlFile);

					// clear
					Table.AcceptChanges();
				}

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public override void Open()
		{
			if (IsOpened)
				return;

			// make table
			if (Table == null)
			{
				if (Adapter == null)
				{
					if (string.IsNullOrEmpty(_XmlFile)) throw new RuntimeException("Table, adapter, or file is not defined.");

					// dataset
					var ds = new DataSet();
					ds.Locale = CultureInfo.CurrentCulture; // CA

					// read schema
					if (!string.IsNullOrEmpty(XmlSchema))
						ds.ReadXmlSchema(XmlSchema);

					// read data
					ds.ReadXml(_XmlFile, XmlReadMode);
					_XmlFileTime = _XmlFileTime = File.GetLastWriteTime(_XmlFile);

					// accept data
					ds.AcceptChanges();

					// table
					Table = GetTable(ds, TableName);
				}
				else
				{
					if (Adapter.SelectCommand == null) throw new RuntimeException("Adapter select command is null.");

					Table = new DataTable();
					Table.Locale = CultureInfo.CurrentCulture; // CA
				}
			}

			// fill table
			Fill();

			// pass 1: collect the columns
			IList<Meta> metas;
			if (Columns == null)
			{
				// collect/filter table columns to be shown
				int Count = Math.Min(Table.Columns.Count, Settings.Default.MaximumPanelColumnCount);
				metas = new List<Meta>(Count);
				int nCollected = 0;
				foreach (DataColumn column in Table.Columns)
				{
					// skip hidden not calculated columns
					if (column.ColumnMapping == MappingType.Hidden && column.Expression.Length == 0)
						continue;

					// skip not linear data
					if (!Converter.IsLinearType(column.DataType))
						continue;

					// infer column meta data
					Meta meta = new Meta(column.ColumnName);
					meta.Kind = FarColumn.DefaultColumnKinds[nCollected];
					metas.Add(meta);
					++nCollected;
					if (nCollected >= Count)
						break;
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

			switch (Far.Net.Message(Res.AskSaveModified, "Save", MessageOptions.YesNoCancel))
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
			BuildDeleteCommand();

			var Files = Explorer.Cache;

			if (args.UI && 0 != (Far.Net.Confirmations & FarConfirmations.Delete))
			{
				if (0 != Far.Net.Message("Delete selected record(s)?", Res.Delete, MessageOptions.None, new string[] { Res.Delete, Res.Cancel }))
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
			if (UserWants == UserAction.CtrlR)
			{
				if (CanClose())
				{
					if (Adapter != null)
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
			BuildUpdateCommand();

			OpenFileActor(file);
		}
		int RecordLimit = Settings.Default.MaximumPanelFileCount;
		// 0-based internally and in UI: 'offset' means "skip 'offset' records"
		int RecordOffset;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void Fill()
		{
			if (Adapter != null)
				Adapter.Fill(RecordOffset, RecordLimit, Table);

			var Files = Explorer.Cache;
			Files.Clear();

			//! Propagate sort and filter to the default view (which is reset on some events).
			if (ViewSort != null && Table.DefaultView.Sort != ViewSort)
			{
				try
				{
					Table.DefaultView.Sort = ViewSort;
				}
				catch (Exception ex)
				{
					ViewSort = null;
					Far.Net.ShowError("Invalid sort expression", ex);
				}
			}
			if (ViewRowFilter != null && Table.DefaultView.RowFilter != ViewRowFilter)
			{
				try
				{
					Table.DefaultView.RowFilter = ViewRowFilter;
				}
				catch (Exception ex)
				{
					ViewRowFilter = null;
					Far.Net.ShowError("Invalid filter expression", ex);
				}
			}

			// table rows or view rows on sort or filter
			if (Table.DefaultView.RowFilter.Length == 0 && Table.DefaultView.Sort.Length == 0)
			{
				foreach (DataRow dr in Table.Rows)
					Files.Add(new DataRowFile(dr, Map));
			}
			else
			{
				foreach (DataRowView drv in Table.DefaultView)
					Files.Add(new DataRowFile(drv.Row, Map));
			}
		}
		internal override void ShowHelpForPanel()
		{
			Far.Net.ShowHelpTopic("DataPanel");
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
			if (Adapter == null || Adapter.DeleteCommand != null)
				return;

			EnsureBuilder();
			Adapter.DeleteCommand = _Builder.GetDeleteCommand();
		}
		/// <summary>
		/// Builds INSERT command.
		/// </summary>
		public void BuildInsertCommand()
		{
			if (Adapter == null || Adapter.DeleteCommand != null)
				return;

			EnsureBuilder();
			Adapter.InsertCommand = _Builder.GetInsertCommand();
		}
		/// <summary>
		/// Builds UPDATE command.
		/// </summary>
		public void BuildUpdateCommand()
		{
			if (Adapter == null || Adapter.DeleteCommand != null)
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
		public override bool UIKeyPressed(KeyInfo key) //????? docs
		{
			if (key == null) throw new ArgumentNullException("key");
			
			switch (key.VirtualKeyCode)
			{
				case KeyCode.PageDown:
					
					if (key.CtrlAltShift() == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.ShiftPressed))
					{
						OnRangeNext();
						return true;
					}
					
					break;
				
				case KeyCode.PageUp:
					
					if (key.CtrlAltShift() == (ControlKeyStates.LeftCtrlPressed | ControlKeyStates.ShiftPressed))
					{
						OnRangePrevious();
						return true;
					}
					
					break;
			}
			
			return base.UIKeyPressed(key);
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
			OpenFileActor(file);
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

			switch (Far.Net.Message(Res.AskSaveModified, "Save", MessageOptions.YesNoCancel))
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
						if (dt != null && Far.Net.Message(Res.AskSaveModified, "Save", MessageOptions.YesNo) == 0)
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
		void OpenFileActor(FarFile file)
		{
			var memberPanel = OpenFileMembers(file);
			memberPanel.Explorer.CanDeleteFiles = false;
		}
		void OnSort()
		{
			GetValues getWords = delegate
			{
				var list = new List<string>();
				foreach (DataColumn c in Table.Columns)
					list.Add(c.ColumnName);
				return list;
			};

			var ui = new UI.InputBoxEx()
			{
				Title = "Sort",
				Prompt = "Sort column list: [Tab] to complete, comma to separate",
				Text = ViewSort ?? string.Empty,
				History = "DataRecordSort",
				GetWords = getWords
			};
			if (!ui.Show())
				return;

			var text = ui.Text;
			if (text == ViewSort)
				return;

			ViewSort = text;

			UserWants = UserAction.CtrlR;
			UpdateRedraw(true);
		}
		void OnFilter()
		{
			GetValues getWords = delegate
			{
				var list = new List<string>();
				foreach (DataColumn c in Table.Columns)
					list.Add(c.ColumnName);

				list.AddRange(new string[] {
"","and","avg","between","child","convert","count","false","iif","in","is","isnull","len","like","max","min","not","null","or","parent","stdev","substring","sum","trim","true","var",
});

				return list;
			};

			var ui = new UI.InputBoxEx()
			{
				Title = "Filter",
				Prompt = "Filter expression: [Tab] to complete",
				Text = ViewRowFilter ?? string.Empty,
				History = "DataRecordFilter",
				GetWords = getWords
			};
			if (!ui.Show())
				return;

			var text = ui.Text;
			if (text == ViewRowFilter)
				return;

			ViewRowFilter = text;

			UserWants = UserAction.CtrlR;
			UpdateRedraw(true);
		}
		void OnRecordLimit()
		{
			var text = Far.Net.Input("Record limit", "DataRecordLimit", "Data Panel", "" + RecordLimit);
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
		}
		void OnRecordOffset()
		{
			var text = Far.Net.Input("Record offset", "DataRecordOffset", "Data Panel", "" + RecordOffset);
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
		}
		internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
		{
			e.Menu.Add("Sort").Click = delegate { OnSort(); };

			e.Menu.Add("Filter").Click = delegate { OnFilter(); };

			e.Menu.Add(string.Empty).IsSeparator = true;

			if (Adapter != null)
			{
				e.Menu.Add("Next range").Click = delegate { OnRangeNext(); };

				e.Menu.Add("Previous range").Click = delegate { OnRangePrevious(); };

				e.Menu.Add("Record limit").Click = delegate { OnRecordLimit(); };

				e.Menu.Add("Record offset").Click = delegate { OnRecordOffset(); };

				e.Menu.Add(string.Empty).IsSeparator = true;
			}

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
		/// <summary>
		/// Gets or sets the sort column or columns, and sort order. See <see cref="DataView.Sort"/>.
		/// </summary>
		public string ViewSort { get; set; }
		/// <summary>
		/// Gets or sets the expression used to filter which rows are viewed. See <see cref="DataView.RowFilter"/>.
		/// </summary>
		public string ViewRowFilter { get; set; }
	}
}
