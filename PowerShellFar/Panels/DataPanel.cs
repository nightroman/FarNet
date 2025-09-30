using FarNet;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Panel exploring a data table.
/// </summary>
public sealed class DataPanel : TablePanel, IDisposable
{
	new DataExplorer Explorer => (DataExplorer)base.Explorer;

	///
	public DataPanel() : base(new DataExplorer())
	{
		Explorer.Panel = this;

		CurrentLocation = "*";
		SortMode = PanelSortMode.Unsorted; // assume it is sorted in SELECT
		UseSortGroups = false;

		PageLimit = Settings.Default.MaximumPanelFileCount;
	}

	/// <summary>
	/// Opens the panel with the specified table.
	/// </summary>
	/// <param name="table">The data table.</param>
	public static void Open(DataTable table)
	{
		DataPanel panel = new() { Table = table };
		panel.Open();
	}

	/// <summary>
	/// Opens the panel with the specified table.
	/// </summary>
	/// <param name="table">The data table.</param>
	/// <param name="parent">Optional parent panel, the current, when null.</param>
	public static void OpenChild(DataTable table, Panel? parent = null)
	{
		DataPanel panel = new() { Table = table };
		panel.OpenChild(parent);
	}

	DateTime _XmlFileTime;

	/// <summary>
	/// Gets or sets the XML data source file.
	/// </summary>
	public string? XmlFile
	{
		get => _XmlFile;
		set => _XmlFile = string.IsNullOrEmpty(value) ? null : Path.GetFullPath(value);
	}
	string? _XmlFile;

	/// <summary>
	/// Gets or sets the XML schema definition file.
	/// </summary>
	public string? XmlSchema
	{
		get => _XmlSchema;
		set => _XmlSchema = string.IsNullOrEmpty(value) ? null : Path.GetFullPath(value);
	}
	string? _XmlSchema;

	/// <summary>
	/// Gets or sets the table name.
	/// </summary>
	public string? TableName { get; set; }

	/// <summary>
	/// Gets or sets the XML reading mode.
	/// </summary>
	public XmlReadMode XmlReadMode { get; set; }

	/// <summary>
	/// Gets or sets the XML writing mode.
	/// </summary>
	public XmlWriteMode XmlWriteMode { get; set; }

	static DataTable? GetTable(DataSet dataSet, string tableName)
	{
		if (dataSet.Tables.Count == 0)
			throw new InvalidOperationException("Empty data set.");

		if (!string.IsNullOrEmpty(tableName))
			return dataSet.Tables[tableName];

		if (dataSet.Tables.Count == 1)
			return dataSet.Tables[0];

		var menu = Far.Api.CreateListMenu();
		menu.Title = "Tables";
		menu.UsualMargins = true;

		foreach (DataTable table in dataSet.Tables)
			menu.Add(table.TableName);

		int index = menu.Show() ? menu.Selected : 0;
		return dataSet.Tables[index];
	}

	/// <summary>
	/// Database provider factory instance.
	/// </summary>
	public DbProviderFactory? Factory { get; set; }

	/// <summary>
	/// Connection.
	/// </summary>
	public DbConnection? Connection
	{
		get { return Adapter is null || Adapter.SelectCommand is null ? null : Adapter.SelectCommand.Connection; }
	}

	/// <summary>
	/// Data adapter.
	/// You have to set it and configure at least its <c>SelectCommand</c>.
	/// </summary>
	public DbDataAdapter? Adapter { get; set; }

	/// <summary>
	/// A table which records are used as panel items.
	/// </summary>
	/// <remarks>
	/// Normally this table is created, assigned and filled internally.
	/// If an external table is set then <see cref="XmlFile"/> or <see cref="AsSaveData"/> might be useful.
	/// </remarks>
	public DataTable? Table { get; set; }

	readonly DataRowFileMap Map = new();

	/// <summary>
	/// Disposes internal data. Normally it is called internally.
	/// </summary>
	public void Dispose()
	{
		_Builder?.Dispose();
		Table?.Dispose();
	}

	/// <include file='doc.xml' path='doc/ScriptFork/*'/>
	public sealed override bool SaveData()
	{
		if (AsSaveData != null)
			return LanguagePrimitives.IsTrue(AsSaveData.InvokeReturnAsIs(this, null));
		else
			return DoSaveData();
	}

	/// <summary>
	/// Called by <see cref="SaveData"/>.
	/// </summary>
	public ScriptBlock? AsSaveData { get; set; }

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
				//! may fail ~ Dynamic SQL generation is not supported against a SelectCommand that does not return any key column information.
				// _221127_1221: Dik.sqlite -- table with no PK -- open a record -- change data -- save

				Adapter.Update(Table!);
			}
			else if (!string.IsNullOrEmpty(_XmlFile))
			{
				// conflict?
				if (_XmlFileTime != DateTime.MinValue && File.Exists(_XmlFile) && _XmlFileTime != File.GetLastWriteTime(_XmlFile))
				{
					Far.Api.Message("Cannot save because the source file is modified.", "Conflict");
					return false;
				}

				// write
				if (Table!.DataSet is null) //! user Table.DataSet can be null
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
			A.MyError(ex);
		}
		catch (DbException ex)
		{
			A.MyError(ex);
		}
		return false;
	}

	///
	protected override string DefaultTitle => string.IsNullOrEmpty(Table!.TableName) ? "Data Table" : "Table " + Table.TableName;

	/// <summary>
	/// Fills data table and shows the panel.
	/// </summary>
	public override void Open()
	{
		if (IsOpened)
			return;

		// make table
		if (Table is null)
		{
			if (Adapter is null)
			{
				if (string.IsNullOrEmpty(_XmlFile)) throw new RuntimeException("Table, adapter, or file is not defined.");

				// dataset
				var ds = new DataSet
				{
					Locale = CultureInfo.CurrentCulture // CA
				};

				// read schema
				if (!string.IsNullOrEmpty(XmlSchema))
					ds.ReadXmlSchema(XmlSchema);

				// read data
				ds.ReadXml(_XmlFile, XmlReadMode);
				_XmlFileTime = File.GetLastWriteTime(_XmlFile);

				// accept data
				ds.AcceptChanges();

				// table
				Table = GetTable(ds, TableName!);
			}
			else
			{
				if (Adapter.SelectCommand is null) throw new RuntimeException("Adapter select command is null.");

				Table = new DataTable
				{
					Locale = CultureInfo.CurrentCulture // CA
				};
			}
		}

		// fill and drop the flag avoiding 2nd call on opening
		Fill();
		NeedsNewFiles = false;

		// pass 1: collect the columns
		IList<Meta> metas;
		if (Columns is null)
		{
			// collect/filter table columns to be shown
			int Count = Math.Min(Table!.Columns.Count, Settings.Default.MaximumPanelColumnCount);
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
				var meta = new Meta(column.ColumnName)
				{
					Kind = FarColumn.DefaultColumnKinds[nCollected]
				};
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
			var column = Table!.Columns[meta.Property!]!;

			switch (meta.Kind![0])
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

	//! Avoid Table.GetChanges(). It clones the table internally.
	// Also when called without DataSet it may fail constraints.
	static bool TableHasChanges(DataTable table)
	{
		foreach (DataRow row in table.Rows)
			if (row.RowState != DataRowState.Unchanged)
				return true;
		return false;
	}

	///??
	protected override bool CanClose()
	{
		if (!TableHasChanges(Table!))
			return true;

		switch (Far.Api.Message(Res.AskSaveModified, "Save", MessageOptions.YesNoCancel))
		{
			case 0:
				return SaveData();
			case 1:
				Table!.RejectChanges(); // to avoid request
				return true;
			default:
				return false;
		}
	}

	internal void DoDeleteFiles(DeleteFilesEventArgs args)
	{
		BuildDeleteCommand();

		var Files = Explorer.Cache;

		if (args.UI && 0 != (long)Far.Api.GetSetting(FarSetting.Confirmations, "Delete"))
		{
			if (0 != Far.Api.Message("Delete selected record(s)?", Res.Delete, MessageOptions.None, [Res.Delete, Res.Cancel]))
				return;
		}

		ToUpdateData = true;

		foreach (var file in args.Files)
		{
			if (file.Data is not DataRow dr)
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

	bool ToUpdateData { get; set; } = true;

	internal IList<FarFile>? Explore(GetFilesEventArgs args)
	{
		ArgumentNullException.ThrowIfNull(args);

		var Files = Explorer.Cache;

		// refill
		if (NeedsNewFiles)
		{
			if (CanClose())
			{
				if (Adapter != null)
					Table!.Clear();
				Fill();
			}
		}

		// no job?
		if (!ToUpdateData && !NeedsNewFiles)
			return Files;

		// refresh data
		for (int iFile = Files.Count; --iFile >= 0;)
		{
			FarFile f = Files[iFile];
			if (f.Data is not DataRow Row || Row.RowState == DataRowState.Deleted || Row.RowState == DataRowState.Detached)
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
	/// <param name="file">The file to open.</param>
	public override void OpenFile(FarFile file)
	{
		BuildUpdateCommand();

		OpenFileActor(file);
	}

	void Fill()
	{
		Adapter?.Fill(PageOffset, PageLimit, Table!);

		var Files = Explorer.Cache;
		Files.Clear();

		//! Propagate sort and filter to the default view (which is reset on some events).
		if (ViewSort != null && Table!.DefaultView.Sort != ViewSort)
		{
			try
			{
				Table.DefaultView.Sort = ViewSort;
			}
			catch (Exception ex)
			{
				ViewSort = null;
				Far.Api.ShowError("Invalid sort expression", ex);
			}
		}
		if (ViewRowFilter != null && Table!.DefaultView.RowFilter != ViewRowFilter)
		{
			try
			{
				Table.DefaultView.RowFilter = ViewRowFilter;
			}
			catch (Exception ex)
			{
				ViewRowFilter = null;
				Far.Api.ShowError("Invalid filter expression", ex);
			}
		}

		// table rows or view rows on sort or filter
		if (Table!.DefaultView.RowFilter!.Length == 0 && Table.DefaultView.Sort.Length == 0)
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
		Entry.Instance.ShowHelpTopic(HelpTopic.DataPanel);
	}

	// Command builder
	DbCommandBuilder? _Builder;

	void EnsureBuilder()
	{
		if (_Builder != null)
			return;

		if (Factory is null)
			throw new RuntimeException("Cannot create a command builder because this.Factory is null.");

		Adapter ??= Factory.CreateDataAdapter();

		_Builder = Factory.CreateCommandBuilder();
		_Builder!.DataAdapter = Adapter;
	}

	/// <summary>
	/// Builds DELETE command.
	/// </summary>
	public void BuildDeleteCommand()
	{
		if (Adapter is null || Adapter.DeleteCommand != null)
			return;

		EnsureBuilder();
		Adapter.DeleteCommand = _Builder!.GetDeleteCommand();
	}

	/// <summary>
	/// Builds INSERT command.
	/// </summary>
	public void BuildInsertCommand()
	{
		if (Adapter is null || Adapter.DeleteCommand != null)
			return;

		EnsureBuilder();
		Adapter.InsertCommand = _Builder!.GetInsertCommand();
	}

	/// <summary>
	/// Builds UPDATE command.
	/// </summary>
	public void BuildUpdateCommand()
	{
		if (Adapter is null || Adapter.DeleteCommand != null)
			return;

		EnsureBuilder();

		try
		{
			Adapter.UpdateCommand = _Builder!.GetUpdateCommand();
		}
		catch (Exception ex)
		{
			// ~ Dynamic SQL generation for the UpdateCommand is not supported against a SelectCommand that does not return any key column information.
			// _221127_1221: Dik.sqlite -- table with no PK -- open a record
			Log.TraceException(ex);
			Adapter.UpdateCommand = null;
		}
	}

	internal override string HelpMenuTextOpenFileMembers => "Edit row data";

	internal void DoCreateFile()
	{
		BuildInsertCommand();

		// add new row to the table
		DataRow dr = Table!.NewRow();
		Table.Rows.Add(dr);

		// new dummy file, do not add, it can be canceled
		var file = new DataRowFile(dr, Map); //_110330_175246

		// open the record panel
		OpenFileActor(file);
	}

	///
	protected override bool CanCloseChild()
	{
		if (Child is not MemberPanel mp)
			return true;

		if (mp.Value.BaseObject is not DataRow dr)
			return true;

		var xRowState = dr.RowState;
		if (0 == (xRowState & (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified)))
			return true;

		switch (Far.Api.Message(Res.AskSaveModified, "Save", MessageOptions.YesNoCancel))
		{
			case 0:
				// save data, update the table
				var result = SaveData();

				// now we can add a new file for the new record
				if (xRowState == DataRowState.Added) //_110330_175246
				{
					var file = new DataRowFile(dr, Map);
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

	/// <summary>
	/// Disposes data on exit.
	/// </summary>
	public override void UIClosed()
	{
		try
		{
			if (Adapter != null)
			{
				using var dt = Table!.GetChanges();
				if (dt != null && Far.Api.Message(Res.AskSaveModified, "Save", MessageOptions.YesNo) == 0)
					SaveData();
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
		var memberPanel = OpenFileMembers(file)!;
		memberPanel.MyExplorer.SkipDeleteFiles = true;
	}

	void OnSort()
	{
		IEnumerable getWords()
		{
			var list = new List<string>();
			foreach (DataColumn c in Table!.Columns)
				list.Add(c.ColumnName);
			return list;
		}

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

		NeedsNewFiles = true;
		UpdateRedraw(true);
	}

	void OnFilter()
	{
		IEnumerable getWords()
		{
			var list = new List<string>();
			foreach (DataColumn c in Table!.Columns)
				list.Add(c.ColumnName);

			list.AddRange(s_words);

			return list;
		}

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

		NeedsNewFiles = true;
		UpdateRedraw(true);
	}

	void OnPageLimit()
	{
		var text = Far.Api.Input("Record limit", "DataRecordLimit", "Data Panel", "" + PageLimit);
		if (string.IsNullOrEmpty(text))
			return;

		if (!int.TryParse(text, out int value) || value < 1)
		{
			A.MyMessage("Invalid number");
			return;
		}

		PageLimit = value;

		NeedsNewFiles = true;
		UpdateRedraw(true);
	}

	void OnPageOffset()
	{
		var text = Far.Api.Input("Record offset", "DataRecordOffset", "Data Panel", "" + PageOffset);
		if (string.IsNullOrEmpty(text))
			return;

		if (!int.TryParse(text, out int value) || value < 0)
		{
			A.MyMessage("Invalid number");
			return;
		}

		PageOffset = value;

		NeedsNewFiles = true;
		UpdateRedraw(true);
	}

	internal override void HelpMenuInitItems(HelpMenuItems items, PanelMenuEventArgs e)
	{
		e.Menu.Add("Sort").Click = delegate { OnSort(); };

		e.Menu.Add("Filter").Click = delegate { OnFilter(); };

		e.Menu.Add(string.Empty).IsSeparator = true;

		if (Adapter != null)
		{
			e.Menu.Add("Page limit").Click = delegate { OnPageLimit(); };

			e.Menu.Add("Page offset").Click = delegate { OnPageOffset(); };

			e.Menu.Add(string.Empty).IsSeparator = true;
		}

		items.Create ??= new SetItem()
		{
			Text = "&New row",
			Click = delegate { UICreate(); }
		};

		items.Delete ??= new SetItem()
		{
			Text = "&Delete row(s)",
			Click = delegate { UIDelete(false); }
		};

		base.HelpMenuInitItems(items, e);
	}

	/// <summary>
	/// Gets or sets the sort column or columns, and sort order. See <see cref="DataView.Sort"/>.
	/// </summary>
	public string? ViewSort { get; set; }

	/// <summary>
	/// Gets or sets the expression used to filter which rows are viewed. See <see cref="DataView.RowFilter"/>.
	/// </summary>
	public string? ViewRowFilter { get; set; }

	private static readonly string[] s_words = [
		"",
		"and",
		"avg",
		"between",
		"child",
		"convert",
		"count",
		"false",
		"iif",
		"in",
		"is",
		"isnull",
		"len",
		"like",
		"max",
		"min",
		"not",
		"null",
		"or",
		"parent",
		"stdev",
		"substring",
		"sum",
		"trim",
		"true",
		"var",
	];
}
