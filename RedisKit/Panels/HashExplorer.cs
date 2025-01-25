using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisKit.Panels;

sealed class HashExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("29ae0735-2a00-43be-896b-9e2e8a67d658");

	readonly RedisKey _key;
	readonly bool _eol;

	internal RedisKey Key => _key;
	internal bool Eol => _eol;

	public HashExplorer(IDatabase database, RedisKey key, bool eol=false) : base(database, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanGetContent = true;
		CanSetText = true;

		_key = key;
		_eol = eol;
	}

	public override Panel CreatePanel()
	{
		return new HashPanel(this);
	}

	protected override string PanelTitle()
	{
		return $"Hash {_key}";
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var hash = Database.HashGetAll(_key);
		var now = DateTime.Now;

		foreach (HashEntry item in hash)
		{
			var file = new SetFile
			{
				Name = (string)item.Name!,
				Description = (string?)item.Value,
				Data = item,
			};

			if (_eol)
			{
				var ttl = Database.HashFieldGetTimeToLive(_key, [item.Name]);
				if (ttl[0] > 0)
					file.LastWriteTime = now.AddMilliseconds(ttl[0]);
			}

			yield return file;
		}
	}

	public override void CloneFile(CloneFileEventArgs args)
	{
		var newName = (string)args.Data!;
		var item = (HashEntry)args.File.Data!;
		Database.HashSet(_key, newName, item.Value);
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
		var newName = (string)args.Data!;
		Database.HashSet(_key, newName, string.Empty);
		args.PostName = newName;
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		var names = args.Files.Select(f => new RedisValue(f.Name)).ToArray();
		long res = Database.HashDelete(_key, names);
		if (res != names.Length)
			args.Result = JobResult.Incomplete;
	}

	public override void RenameFile(RenameFileEventArgs args)
	{
		var newName = (string)args.Data!;
		var item = (HashEntry)args.File.Data!;
		Database.HashSet(_key, newName, item.Value);
		Database.HashDelete(_key, item.Name);
		args.PostName = newName;
	}

	public override void GetContent(GetContentEventArgs args)
	{
		var item = (HashEntry)args.File.Data!;
		var text = (string?)item.Value;

		args.CanSet = true;
		args.UseText = text;
	}

	public override void SetText(SetTextEventArgs args)
	{
		var item = (HashEntry)args.File.Data!;
		Database.HashSet(_key, item.Name, args.Text);
	}
}
