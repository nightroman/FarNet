
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace PowerShellFar
{
	class FileMap
	{
		public FileMap()
		{
			Columns = new List<Meta>();
		}
		public Meta Name { get; set; }
		public Meta Owner { get; set; }
		public Meta Description { get; set; }
		public Meta Length { get; set; }
		public Meta CreationTime { get; set; }
		public Meta LastWriteTime { get; set; }
		public Meta LastAccessTime { get; set; }
		public List<Meta> Columns { get; private set; }
	}

	class FileColumnEnumerator : System.Collections.IEnumerator
	{
		readonly PSObject Value;
		readonly List<Meta> Columns;
		int Index = -1;
		public FileColumnEnumerator(PSObject value, List<Meta> columns)
		{
			Value = value;
			Columns = columns;
		}
		public object Current
		{
			get
			{
				//! Use GetString(), to use format string, enumeration limit, and etc.
				return Columns[Index].GetString(Value);
			}
		}
		public void Reset()
		{
			Index = -1;
		}
		public bool MoveNext()
		{
			return ++Index < Columns.Count;
		}
	}

	class FileColumnCollection : My.SimpleCollection
	{
		readonly PSObject Value;
		readonly List<Meta> Columns;
		public FileColumnCollection(PSObject value, List<Meta> columns)
		{
			Value = value;
			Columns = columns;
		}
		public override int Count
		{
			get { return Columns.Count; }
		}
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return new FileColumnEnumerator(Value, Columns);
		}
	}

	class MapFile : FarFile
	{
		protected PSObject Value { get; private set; }
		readonly FileMap Map;
		public MapFile(PSObject value, FileMap map)
		{
			Value = value;
			Map = map;
		}
		public override string Name => Map.Name?.GetString(Value);
		public override string Owner => Map.Owner?.GetString(Value);
		public override string Description => Map.Description?.GetString(Value);
		public override long Length => Map.Length == null ? 0 : Map.Length.GetInt64(Value);
		public override DateTime CreationTime => Map.CreationTime == null ? new DateTime() : Map.CreationTime.EvaluateDateTime(Value);
		public override DateTime LastWriteTime => Map.LastWriteTime == null ? new DateTime() : Map.LastWriteTime.EvaluateDateTime(Value);
		public override DateTime LastAccessTime => Map.LastAccessTime == null ? new DateTime() : Map.LastAccessTime.EvaluateDateTime(Value);
		public override System.Collections.ICollection Columns => Map.Columns.Count > 0 ? new FileColumnCollection(Value, Map.Columns) : null;
		public override object Data => Value;
	}

	/// <summary>
	/// Provider item map file.
	/// </summary>
	sealed class ItemMapFile : MapFile
	{
		public override string Name
		{
			get { return Value.Properties["PSChildName"].Value.ToString(); }
		}
		public override FileAttributes Attributes
		{
			get { return ((bool)Value.Properties["PSIsContainer"].Value) ? FileAttributes.Directory : 0; }
		}
		public ItemMapFile(PSObject value, FileMap map) : base(value, map) { }
	}

	/// <summary>
	/// System item map file.
	/// </summary>
	sealed class SystemMapFile : MapFile
	{
		public SystemMapFile(PSObject value, FileMap map) : base(value, map) { }
		public override string Name => ((FileSystemInfo)Value.BaseObject).Name;
		public override DateTime CreationTime => ((FileSystemInfo)Value.BaseObject).CreationTime;
		public override DateTime LastAccessTime => ((FileSystemInfo)Value.BaseObject).LastAccessTime;
		public override DateTime LastWriteTime => ((FileSystemInfo)Value.BaseObject).LastWriteTime;
		public override long Length => !(Value.BaseObject is FileInfo file) ? 0 : file.Length;
		public override FileAttributes Attributes => ((FileSystemInfo)Value.BaseObject).Attributes;
	}

}
