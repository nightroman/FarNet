using FarNet;
using RedisKit.Commands;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RedisKit.Panels;

class KeysExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("5b2529ff-5482-46e5-b730-f9bdecaab8cc");
	readonly string? _pattern;

	public string? Colon { get; }
	public string? Prefix { get; }

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

	public KeysExplorer(IDatabase database, string colon, string? prefix) : this(database)
	{
		if (colon.Length == 0)
			throw new ArgumentException("Colon cannot be empty.");

		Colon = colon;

		if (prefix is { })
		{
			Prefix = prefix;
			_pattern = ConvertPrefixToPattern(Prefix);
		}
	}

	KeysExplorer(IDatabase database) : base(database, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanOpenFile = true;
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
		Prefix is null ? (string)key! : ((string)key!)[Prefix.Length..];

	public override string ToString()
	{
		var name = Colon is { } ? "Tree" : "Keys";
		var info = Prefix ?? _pattern ?? Database.Multiplexer.Configuration;
		return $"{name} {info}";
	}

	public Files.KeyInput GetNameInput(FarFile file)
	{
		string name = file.IsDirectory ? file.DataFolder().Prefix : (string)file.DataKey().Key!;
		if (Prefix is { })
			name = name[Prefix.Length..];

		return new(name, Prefix);
	}

	public override Panel CreatePanel()
	{
		return new KeysPanel(this);
	}

	public override void EnterPanel(Panel panel)
	{
		base.EnterPanel(panel);

		if (Colon is { })
		{
			if (Prefix is { })
				panel.DotsMode = PanelDotsMode.Dots;
			else
				panel.DotsMode = PanelDotsMode.Auto;
		}
	}

	public override IEnumerable<FarFile> GetFiles(GetFilesEventArgs args)
	{
		var server = GetServer();
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
					var nameAndColon = name[..(index + Colon.Length)];
					if (folders!.TryGetValue(nameAndColon, out int count))
						folders[nameAndColon] = count + 1;
					else
						folders.Add(nameAndColon, 1);
					continue;
				}
			}

			var file = new SetFile
			{
				Name = ToFileName(key!),
				Length = 1,
				Data = new Files.FileDataKey(key),
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
			foreach (var (nameAndColon, count) in folders)
			{
				yield return new SetFile
				{
					Name = $"{nameAndColon} ({count})",
					IsDirectory = true,
					Length = count,
					Data = new Files.FileDataFolder(Prefix + nameAndColon),
				};
			}
		}
	}

	public override Explorer? OpenFile(OpenFileEventArgs args)
	{
		var key = args.File.DataKey().Key;
		var type = Database.KeyType(key);
		return type switch
		{
			RedisType.Hash => new HashExplorer(Database, key),
			RedisType.List => new ListExplorer(Database, key),
			RedisType.Set => new SetExplorer(Database, key),
			RedisType.String => null,
			_ => throw new ModuleException($"Not implemented for {type}."),
		};
	}

	public override Explorer? ExploreDirectory(ExploreDirectoryEventArgs args)
	{
		var prefix = args.File.DataFolder().Prefix;
		return new KeysExplorer(Database, Colon!, prefix);
	}

	public override Explorer? ExploreParent(ExploreParentEventArgs args)
	{
		if (Colon is null || Prefix is null)
			return null;

		args.PostData = new Files.FileDataFolder(Prefix);

		var startIndex = Prefix.Length - Colon.Length - Colon.Length;
		if (startIndex < 0)
			return new KeysExplorer(Database, Colon, null);

		var index = Prefix.LastIndexOf(Colon, startIndex);
		if (index < 0)
			return new KeysExplorer(Database, Colon, null);

		return new KeysExplorer(Database, Colon, Prefix[..(index + Colon.Length)]);
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

		var key = args.File.DataKey().Key;
		var key2 = new RedisKey(args.DataName().Name);

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
		var key = new RedisKey(args.DataName().Name);
		Database.StringSet(key, string.Empty);
		args.PostData = new Files.FileDataKey(key);
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
		IServer? server = null;

		var keys = new List<RedisKey>();
		foreach (var file in args.Files)
		{
			if (file.IsDirectory)
			{
				server ??= GetServer();

				var prefix = file.DataFolder().Prefix;
				var pattern = ConvertPrefixToPattern(prefix);
				keys.AddRange(server.Keys(Database.Database, pattern));
			}
			else
			{
				keys.Add(file.DataKey().Key);
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
		{
			var prefix1 = args.File.DataFolder().Prefix;
			var prefix2 = args.DataName().Name;
			if (prefix1 == prefix2)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			var server = GetServer();
			var keys = server.Keys(Database.Database, ConvertPrefixToPattern(prefix1));
			var keyPairs = new List<(RedisKey, RedisKey)>();
			int countExisting = 0;
			foreach (var key1 in keys)
			{
				var name1 = (string)key1!;
				var name2 = prefix2 + name1[prefix1.Length..];
				var key2 = new RedisKey(name2);
				if (Database.KeyExists(key2))
					++countExisting;

				keyPairs.Add((key1, key2));
			}

			if (countExisting > 0 && args.Mode != ExplorerModes.Silent)
			{
				if (0 != Far.Api.Message(
					$"Found {countExisting} existing keys.\nContinue renaming?",
					$"Renaming {keyPairs.Count} keys",
					MessageOptions.YesNo))
				{
					args.Result = JobResult.Ignore;
					return;
				}
			}

			int countDone = 0;
			foreach (var (key1, key2) in keyPairs)
			{
				if (Database.KeyRename(key1, key2))
					++countDone;
			}

			if (countDone != keyPairs.Count && args.Mode != ExplorerModes.Silent)
			{
				Far.Api.Message(
					$"Renamed {countDone}/{keyPairs.Count} keys.",
					"Renaming keys");
			}

			args.PostData = new Files.FileDataFolder(prefix2);
		}
		else
		{
			var key1 = args.File.DataKey().Key;
			var key2 = new RedisKey(args.DataName().Name);
			Database.KeyRename(key1, key2);
			args.PostData = new Files.FileDataKey(key2);
		}
	}

	public override void GetContent(GetContentEventArgs args)
	{
		var key = args.File.DataKey().Key;
		var text = (string?)Database.StringGet(key);
		if (text is null)
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
		var key = args.File.DataKey().Key;
		Database.StringSet(key, args.Text);
	}
}
