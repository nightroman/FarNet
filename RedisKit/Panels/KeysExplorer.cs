using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RedisKit;

class KeysExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("5b2529ff-5482-46e5-b730-f9bdecaab8cc");
    readonly string? _pattern;

    public KeysExplorer(IDatabase database, string? mask) : base(database, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanGetContent = true;
		CanSetText = true;

		if (!string.IsNullOrEmpty(mask))
		{
			if (mask.Contains('[') || mask.Contains(']'))
			{
				_pattern = mask;
			}
			else if (mask.Contains('*') || mask.Contains('?'))
			{
				_pattern = mask.Replace("\\", "\\\\");
			}
			else
			{
				Prefix = mask;
				_pattern = mask.Replace("\\", "\\\\") + '*';
			}
		}
	}

	public string? Prefix { get; }

	public override string ToString()
	{
		var info = Prefix ?? _pattern ?? Database.Multiplexer.Configuration;
		return $"Keys {info}";
	}

	RedisKey ToKey(string name) =>
		Prefix is null ? name : Prefix + name;

	string ToFileName(RedisKey key) =>
		Prefix is null ? (string)key! : ((string)key!).Substring(Prefix.Length);

	public override Panel CreatePanel()
	{
		return new KeysPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var server = Database.Multiplexer.GetServers()[Database.Database];
		var keys = server.Keys(Database.Database, _pattern);
		var now = DateTime.Now;

		foreach (RedisKey key in keys)
		{
			var file = new SetFile
            {
                Name = ToFileName(key!),
                Data = key,
            };

			var type = Database.KeyType(key);
			switch(type)
			{
				case RedisType.String:
					file.Owner = "*";
					break;
				case RedisType.Hash:
					file.Owner = "H";
					break;
				case RedisType.List:
					file.Owner = "L";
					break;
				case RedisType.Set:
					file.Owner = "S";
					break;
            }

            var ttl = Database.KeyTimeToLive(key);
			if (ttl.HasValue)
				file.LastWriteTime = now + ttl.Value;

			yield return file;
		}
	}

	public override void CloneFile(CloneFileEventArgs args)
	{
		var key = (RedisKey)args.File.Data!;
		var key2 = ToKey((string)args.Data!);

		var type = Database.KeyType(key);
		switch (type)
		{
			case RedisType.String:
				var str = (string?)Database.StringGet(key);
				if (str == null)
					return;

				Database.KeyDelete(key2);
				Database.StringSet(key2, str);
				break;

			case RedisType.Hash:
				var hash = Database.HashGetAll(key);
				if (hash.Length == 0)
					return;

				Database.KeyDelete(key2);
				Database.HashSet(key2, hash);
				break;

			case RedisType.List:
				var list = Database.ListRange(key);
				if (list.Length == 0)
					return;

				Database.KeyDelete(key2);
				Database.ListRightPush(key2, list);
				break;

			case RedisType.Set:
				var set = Database.SetMembers(key);
				if (set.Length == 0)
					return;

				Database.KeyDelete(key2);
				Database.SetAdd(key2, set);
				break;

			default:
				throw new ModuleException($"Not implemented for {type}.");
		}
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
		var newName = (string)args.Data!;
		var newKey = ToKey(newName);
		Database.StringSet(newKey, string.Empty);
		args.PostName = newName;
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		var keys = args.Files.Select(x => (RedisKey)x.Data!).ToArray();
        try
        {
            long res = Database.KeyDelete(keys);
			if (res != keys.Length)
				throw new Exception($"Deleted {res} of {keys.Length} keys.");
        }
		catch (Exception ex)
		{
			if (args.UI)
				Far.Api.Message(ex.Message, Host.MyName, MessageOptions.LeftAligned | MessageOptions.Warning);

			args.Result = JobResult.Incomplete;
		}
    }

    public override void RenameFile(RenameFileEventArgs args)
	{
        var key = (RedisKey)args.File.Data!;
        var key2 = ToKey((string)args.Data!);
        Database.KeyRename(key, key2);
        args.PostName = key2;
    }

    public override void GetContent(GetContentEventArgs args)
    {
        var key = (RedisKey)args.File.Data!;
		var text = (string)Database.StringGet(key)!;
		if (text == null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.CanSet = true;
		args.UseText = text;
		args.UseFileExtension = EditCommand.GetFileExtension(key.ToString());
	}

    public override void SetText(SetTextEventArgs args)
    {
        var key = (RedisKey)args.File.Data!;
		Database.StringSet(key, args.Text);
    }

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var key = (RedisKey)args.File.Data!;
		var type = Database.KeyType(key);
		switch (type)
		{
			case RedisType.Hash:
				return new HashExplorer(Database, key);

			case RedisType.List:
				return new ListExplorer(Database, key);

			case RedisType.Set:
				return new SetExplorer(Database, key);

			case RedisType.String:
				return null;

			default:
				throw new ModuleException($"Not implemented for {type}.");
		}
	}
}
