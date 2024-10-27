using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RedisKit.Panels;

class ListExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("be46affb-dd5c-436b-99c3-197dfd6e9d1f");
	static readonly RedisValue s_deleted = "<DELETED>";

	readonly RedisKey _key;
	internal RedisKey Key => _key;

	public ListExplorer(IDatabase database, RedisKey key) : base(database, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanGetContent = true;
		CanSetText = true;

		_key = key;
	}

	public override string ToString()
	{
		return $"List {_key}";
	}

	public override Panel CreatePanel()
	{
		return new ListPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var list = Database.ListRange(_key);
		var index = -1;
		foreach (RedisValue item in list)
		{
			++index;
			var file = new SetFile
			{
				Name = (string)item!,
				Length = index,
				Data = item,
			};

			yield return file;
		}
	}

	bool IgnoreChanged(long index, RedisValue value, ExplorerEventArgs args)
	{
		var value2 = Database.ListGetByIndex(_key, index);
		if (value == value2)
			return false;

		args.Result = JobResult.Ignore;
		if (args.UI)
			Far.Api.Message("Cannot apply changes, the list was changed externally.", Host.MyName);

		return true;
	}

	public override void CloneFile(CloneFileEventArgs args)
	{
		var index = args.File.Length;
		var value = (RedisValue)args.File.Data!;
		if (IgnoreChanged(index, value, args))
			return;

		var newName = (string)args.Data!;
		Database.ListSetByIndex(_key, index, s_deleted);
		Database.ListInsertBefore(_key, s_deleted, newName);
		Database.ListSetByIndex(_key, index + 1, value);
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
		var newName = (string)args.Data!;
		Database.ListRightPush(_key, newName);
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		foreach (var file in args.Files)
		{
			if (IgnoreChanged(file.Length, (RedisValue)file.Data!, args))
				return;
		}

		foreach (var file in args.Files)
		{
			var index = file.Length;
			Database.ListSetByIndex(_key, index, s_deleted);
		}

		long res = Database.ListRemove(_key, s_deleted);
		if (res != args.Files.Count)
			args.Result = JobResult.Incomplete;
	}

	public override void RenameFile(RenameFileEventArgs args)
	{
		var index = args.File.Length;
		var value = (RedisValue)args.File.Data!;
		if (IgnoreChanged(index, value, args))
			return;

		var newName = (string)args.Data!;
		Database.ListSetByIndex(_key, index, newName);
		args.PostName = newName;
	}

	public override void GetContent(GetContentEventArgs args)
	{
		var value = (RedisValue)args.File.Data!;
		args.CanSet = true;
		args.UseText = (string?)value;
	}

	public override void SetText(SetTextEventArgs args)
	{
		var index = args.File.Length;
		var value = (RedisValue)args.File.Data!;
		if (IgnoreChanged(index, value, args))
			return;

		Database.ListSetByIndex(_key, index, args.Text);
	}
}
