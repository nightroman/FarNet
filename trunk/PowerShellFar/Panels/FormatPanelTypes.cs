/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FarNet;

namespace PowerShellFar
{
	class ObjectFileMap
	{
		public ObjectFileMap()
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

	class ObjectColumnEnumerator : System.Collections.IEnumerator
	{
		PSObject Value;
		List<Meta> Columns;
		int Index = -1;

		public ObjectColumnEnumerator(PSObject value, List<Meta> columns)
		{
			Value = value;
			Columns = columns;
		}

		public object Current
		{
			get { return Columns[Index].GetValue(Value); }
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

	class ObjectColumnCollection : My.SimpleCollection
	{
		PSObject Value;
		List<Meta> Columns;

		public ObjectColumnCollection(PSObject value, List<Meta> columns)
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
			return new ObjectColumnEnumerator(Value, Columns);
		}
	}

	class MappedObjectFile : FarFile
	{
		PSObject Value;
		ObjectFileMap Map;

		public MappedObjectFile(PSObject value, ObjectFileMap map)
		{
			Value = value;
			Map = map;
		}

		public override string Name
		{
			get { return Map.Name == null ? null : Map.Name.GetString(Value); }
		}

		public override string Owner
		{
			get { return Map.Owner == null ? null : Map.Owner.GetString(Value); }
		}

		public override string Description
		{
			get { return Map.Description == null ? null : Map.Description.GetString(Value); }
		}

		public override long Length
		{
			get { return Map.Length == null ? 0 : Map.Length.GetInt64(Value); }
		}

		public override DateTime CreationTime
		{
			get { return Map.CreationTime == null ? new DateTime() : Map.CreationTime.EvaluateDateTime(Value); }
		}

		public override DateTime LastWriteTime
		{
			get { return Map.LastWriteTime == null ? new DateTime() : Map.LastWriteTime.EvaluateDateTime(Value); }
		}

		public override DateTime LastAccessTime
		{
			get { return Map.LastAccessTime == null ? new DateTime() : Map.LastAccessTime.EvaluateDateTime(Value); }
		}

		public override System.Collections.ICollection Columns
		{
			get
			{
				if (Map.Columns.Count > 0)
					return new ObjectColumnCollection(Value, Map.Columns);
				else
					return null;
			}
		}

		public override object Data
		{
			get
			{
				return Value;
			}
		}
	}

	/// <summary>
	/// Infrastructure. Object file + Attributes.
	/// </summary>
	class MappedItemFile : MappedObjectFile
	{
		public override FileAttributes Attributes { get; set; }

		public MappedItemFile(PSObject value, ObjectFileMap map) : base(value, map) { }
	}

	/// <summary>
	/// Infrastructure. File with Name, Description and Data.
	/// </summary>
	class FormattedObjectFile : FarFile
	{
		public override string Name { get; set; }

		public override string Description { get; set; }
	
		public override object Data { get; set; }
	}

	/// <summary>
	/// Infrastructure. File with Name, Description, Attributes and Data.
	/// </summary>
	class FormattedItemFile : FormattedObjectFile
	{
		public override FileAttributes Attributes { get; set; }
	}
}
