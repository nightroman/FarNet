using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RedisKit;

class KeysExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("5b2529ff-5482-46e5-b730-f9bdecaab8cc");
    readonly string? _pattern;

	public string? Prefix { get; }
	public string? Colon { get; }

	public KeysExplorer(IDatabase database, string? mask) : this(database)
	{
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

	public KeysExplorer(IDatabase database, string colon, string? root) : this(database)
	{
		if (colon.Length == 0)
			throw new ArgumentException("Colon cannot be empty.");

		Colon = colon;

		if (root is { })
		{
			Prefix = root + colon;
			_pattern = ConvertPrefixToPattern(Prefix);
		}
	}

	KeysExplorer(IDatabase database) : base(database, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanGetContent = true;
		CanSetText = true;
	}

	static string ConvertPrefixToPattern(string root)
	{
		return root
			.Replace("\\", "\\\\")
			.Replace("*", "\\*")
			.Replace("?", "\\?")
			.Replace("[", "\\[")
			.Replace("]", "\\]")
			+ '*';
	}

	RedisKey ToKey(string name) =>
		Prefix is null ? name : Prefix + name;

	string ToFileName(RedisKey key) =>
		Prefix is null ? (string)key! : ((string)key!).Substring(Prefix.Length);

	public override string ToString()
	{
		var name = Colon is { } ? "Tree" : "Keys";
		var info = Prefix ?? _pattern ?? Database.Multiplexer.Configuration;
		return $"{name} {info}";
	}

	public override Panel CreatePanel()
	{
		return new KeysPanel(this);
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var server = Database.Multiplexer.GetServers()[Database.Database];
		var keys = server.Keys(Database.Database, _pattern);
		var now = DateTime.Now;

		var folders = Colon is { } ? new Dictionary<string, int>() : null;

		foreach (RedisKey key in keys)
		{
			var name = ToFileName(key!);

			// folder?
			if (Colon is { })
			{
				int index = name.IndexOf(Colon);
				if (index >= 0)
				{
					var folder = name[..index];
					if (folders!.TryGetValue(folder, out int count))
						folders[folder] = count + 1;
					else
						folders.Add(folder, 1);
					continue;
				}
			}

			var file = new SetFile
			{
				Name = ToFileName(key!),
				Data = key,
				Length = 1,
			};

			var type = Database.KeyType(key);
			switch (type)
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

		if (folders is { })
		{
			foreach (var folder in folders)
			{
				var name = folder.Key;
				yield return new SetFile
				{
					IsDirectory = true,
					Name = $"{name}{Colon} ({folder.Value})",
					Data = Prefix + name,
					Length = folder.Value,
				};
			}
		}
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		if (args.File.IsDirectory)
		{
			var path = (string)args.File.Data!;
			return new KeysExplorer(Database, Colon!, path);
		}

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

	public override Explorer? ExploreParent(ExploreParentEventArgs args)
	{
		if (Colon is null || Prefix is null)
			return null;

		var startIndex = Prefix.Length - Colon.Length - Colon.Length;
		if (startIndex < 0)
			return new KeysExplorer(Database, Colon, null);

		var index = Prefix.LastIndexOf(Colon, startIndex);
		if (index < 0)
			return new KeysExplorer(Database, Colon, null);

		return new KeysExplorer(Database, Colon, Prefix[..index]);
	}

	public override Explorer? ExploreRoot(ExploreRootEventArgs args)
	{
		if (Colon is null || Prefix is null)
			return null;

		return new KeysExplorer(Database, Colon, null);
	}

	public override void CloneFile(CloneFileEventArgs args)
	{
		if (args.File.IsDirectory)
		{
			args.Result = JobResult.Ignore;
			return;
		}

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
		IServer? server = null;

		var keys = new List<RedisKey>();
		foreach (var file in args.Files)
		{
			if (file.IsDirectory)
			{
				if (server is null)
					server = Database.Multiplexer.GetServers()[Database.Database];

				var prefix = (string)file.Data! + Colon;
				var pattern = ConvertPrefixToPattern(prefix);
				keys.AddRange(server.Keys(Database.Database, pattern));
			}
			else
			{
				keys.Add((RedisKey)file.Data!);
			}
		}

        try
        {
            long res = Database.KeyDelete([.. keys]);
			if (res != keys.Count)
				throw new Exception($"Deleted {res} of {keys.Count} keys.");
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
		if (args.File.IsDirectory)
			throw new NotImplementedException("Renaming folders is not yet implemented.");

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
}
