using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisKit.Panels;

sealed class SetExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("75bbcfef-c464-4c80-a602-83b15bf404f9");

	readonly RedisKey _key;
	internal RedisKey Key => _key;

	public SetExplorer(IDatabase database, RedisKey key) : base(database, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanGetContent = true;
		CanSetText = true;

		_key = key;
	}

	public override Panel CreatePanel()
	{
		return new SetPanel(this);
	}

	protected override string PanelTitle()
	{
		return $"Set {_key}";
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var set = Database.SetMembers(_key);
		foreach (RedisValue item in set)
		{
			var file = new SetFile
			{
				Name = (string)item!,
				Data = item,
			};

			yield return file;
		}
	}

	public override void CloneFile(CloneFileEventArgs args)
	{
		var newName = (string)args.Data!;
		Database.SetAdd(_key, newName);
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
		var newName = (string)args.Data!;
		Database.SetAdd(_key, newName);
		args.PostName = newName;
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		var names = args.Files.Select(f => new RedisValue(f.Name)).ToArray();
		long res = Database.SetRemove(_key, names);
		if (res != names.Length)
			args.Result = JobResult.Incomplete;
	}

	public override void RenameFile(RenameFileEventArgs args)
	{
		var value = (RedisValue)args.File.Data!;
		var newName = (string)args.Data!;

		Database.SetRemove(_key, value);
		Database.SetAdd(_key, newName);
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
		var value = (RedisValue)args.File.Data!;
		Database.SetRemove(_key, value);
		Database.SetAdd(_key, args.Text);
	}
}
