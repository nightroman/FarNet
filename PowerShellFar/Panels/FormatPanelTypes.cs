using FarNet;
using System.Collections;
using System.Management.Automation;

namespace PowerShellFar;

class FileMap
{
	public FileMap()
	{
		Columns = [];
	}

	public Meta? Name { get; set; }
	public Meta? Owner { get; set; }
	public Meta? Description { get; set; }
	public Meta? Length { get; set; }
	public Meta? CreationTime { get; set; }
	public Meta? LastWriteTime { get; set; }
	public Meta? LastAccessTime { get; set; }
	public List<Meta>? Columns { get; private set; }
}

class FileColumnEnumerator(PSObject value, List<Meta> columns) : IEnumerator
{
	readonly PSObject Value = value;
	readonly List<Meta> Columns = columns;
	int Index = -1;

	public object? Current
	{
		get
		{
			//! Use GetString(), to use format string, enumeration limit, and etc.
			return Columns[Index].GetString(Value);
		}
	}

	public void Reset() => Index = -1;

	public bool MoveNext() => ++Index < Columns.Count;
}

class FileColumnCollection(PSObject value, List<Meta> columns) : MyCollection
{
	readonly PSObject Value = value;
	readonly List<Meta> Columns = columns;

	public override int Count => Columns.Count;

	public override IEnumerator GetEnumerator() => new FileColumnEnumerator(Value, Columns);
}

class MapFile(PSObject value, FileMap map) : FarFile
{
	protected PSObject Value { get; private set; } = value;
	readonly FileMap Map = map;

	public override string Name => Map.Name?.GetString(Value)!;

	public override string? Owner => Map.Owner?.GetString(Value);

	public override string? Description => Map.Description?.GetString(Value);

	public override long Length => Map.Length == null ? 0 : Map.Length.GetValue<long>(Value);

	public override DateTime CreationTime => Map.CreationTime == null ? default : Map.CreationTime.GetValue<DateTime>(Value);

	public override DateTime LastWriteTime => Map.LastWriteTime == null ? default : Map.LastWriteTime.GetValue<DateTime>(Value);

	public override DateTime LastAccessTime => Map.LastAccessTime == null ? default : Map.LastAccessTime.GetValue<DateTime>(Value);

	public override ICollection? Columns => Map.Columns!.Count > 0 ? new FileColumnCollection(Value, Map.Columns) : null;

	public override object? Data => Value;
}

/// <summary>
/// Provider item map file.
/// </summary>
sealed class ItemMapFile(PSObject value, FileMap map) : MapFile(value, map)
{
	public override string Name => Value.Properties["PSChildName"].Value.ToString()!;

	public override FileAttributes Attributes => ((bool)Value.Properties["PSIsContainer"].Value) ? FileAttributes.Directory : 0;
}

/// <summary>
/// System item map file.
/// </summary>
sealed class SystemMapFile(PSObject value, FileMap map) : MapFile(value, map)
{
	public override string Name => ((FileSystemInfo)Value.BaseObject).Name;

	public override DateTime CreationTime => ((FileSystemInfo)Value.BaseObject).CreationTime;

	public override DateTime LastAccessTime => ((FileSystemInfo)Value.BaseObject).LastAccessTime;

	public override DateTime LastWriteTime => ((FileSystemInfo)Value.BaseObject).LastWriteTime;

	public override long Length => Value.BaseObject is not FileInfo file ? 0 : file.Length;

	public override FileAttributes Attributes => ((FileSystemInfo)Value.BaseObject).Attributes;
}
