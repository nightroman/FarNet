
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using FarNet;

namespace PowerShellFar;

class DataRowFileMap
{
	public DataRowFileMap()
	{
		Name = -1;
		Owner = -1;
		Description = -1;
		Length = -1;
		CreationTime = -1;
		LastWriteTime = -1;
		LastAccessTime = -1;
		Columns = new List<int>();
	}

	public int Name { get; set; }
	public int Owner { get; set; }
	public int Description { get; set; }
	public int Length { get; set; }
	public int CreationTime { get; set; }
	public int LastWriteTime { get; set; }
	public int LastAccessTime { get; set; }
	public List<int> Columns { get; private set; }
}

class DataColumnEnumerator : IEnumerator
{
	readonly DataRow Row;
	readonly List<int> Indexes;
	int Index = -1;

	public DataColumnEnumerator(DataRow row, List<int> indexes)
	{
		Row = row;
		Indexes = indexes;
	}

	public object Current => Row[Indexes[Index]];

	public void Reset() => Index = -1;

	public bool MoveNext() => ++Index < Indexes.Count;
}

class DataColumnCollection : My.SimpleCollection
{
	readonly DataRow Row;
	readonly List<int> Indexes;

	public DataColumnCollection(DataRow row, List<int> indexes)
	{
		Row = row;
		Indexes = indexes;
	}

	public override int Count => Indexes.Count;

	public override IEnumerator GetEnumerator() => new DataColumnEnumerator(Row, Indexes);
}

class DataRowFile : FarFile
{
	readonly DataRow Row;
	readonly DataRowFileMap Map;

	public DataRowFile(DataRow row, DataRowFileMap map)
	{
		Row = row;
		Map = map;
	}

	public override string Name
	{
		get
		{
			if (Map.Name >= 0)
			{
				object v = Row[Map.Name];
				if (v != null)
					return v.ToString()!;
			}

			return string.Empty;
		}
	}

	public override string? Owner
	{
		get
		{
			if (Map.Owner >= 0)
			{
				object v = Row[Map.Owner];
				if (v != null)
					return v.ToString();
			}

			return null;
		}
	}

	public override string? Description
	{
		get
		{
			if (Map.Description >= 0)
			{
				object v = Row[Map.Description];
				if (v != null)
					return v.ToString();
			}

			return null;
		}
	}

	public override long Length
	{
		get
		{
			if (Map.Length >= 0)
			{
				object v = Row[Map.Length];
				if (v != DBNull.Value)
					return (long)v;
			}

			return 0;
		}
	}

	public override DateTime CreationTime
	{
		get
		{
			if (Map.CreationTime >= 0)
			{
				object v = Row[Map.CreationTime];
				if (v != DBNull.Value)
					return (DateTime)v;
			}

			return new DateTime();
		}
	}

	public override DateTime LastWriteTime
	{
		get
		{
			if (Map.LastWriteTime >= 0)
			{
				object v = Row[Map.LastWriteTime];
				if (v != DBNull.Value)
					return (DateTime)v;
			}

			return new DateTime();
		}
	}

	public override DateTime LastAccessTime
	{
		get
		{
			if (Map.LastAccessTime >= 0)
			{
				object v = Row[Map.LastAccessTime];
				if (v != DBNull.Value)
					return (DateTime)v;
			}

			return new DateTime();
		}
	}

	public override ICollection? Columns
	{
		get
		{
			if (Map.Columns.Count > 0)
				return new DataColumnCollection(Row, Map.Columns);
			else
				return null;
		}
	}

	public override object Data => Row;
}
