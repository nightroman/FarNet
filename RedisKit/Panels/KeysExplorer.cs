using FarNet;
using RedisKit.Commands;
using StackExchange.Redis;
using System.Runtime.InteropServices;

namespace RedisKit.Panels;

sealed class KeysExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("5b2529ff-5482-46e5-b730-f9bdecaab8cc");

	// Means folders mode and defines the folder separator.
	readonly string? _colon;

	// When folders, stops folder navigation up.
	readonly string? _root;

	// The folder prefix or fixed prefix.
	readonly string? _prefix;

	// The key scan pattern.
	readonly string? _pattern;

	// Read once setting.
	string? _folderSymbols;

	internal string? Colon => _colon;
	internal string? Prefix => _prefix;

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
				_prefix = Location = mask;
				_pattern = mask.Replace("\\", "\\\\") + '*';
			}
		}
	}

	public KeysExplorer(IDatabase database, string colon, string? root, string? prefix) : this(database)
	{
		if (colon.Length == 0)
			throw new ArgumentException("Colon cannot be empty.");

		_colon = colon;

		_root = root;
		_prefix = prefix ?? root;
		if (_prefix is { })
		{
			Location = _prefix;
			_pattern = ConvertPrefixToPattern(_prefix);
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

	string ToFileName(RedisKey key) =>
		_prefix is null ? (string)key! : ((string)key!)[_prefix.Length..];

	public Files.KeyInput GetNameInput(FarFile file)
	{
		string name = file.IsDirectory ? file.DataFolder().Prefix : (string)file.DataKey().Key!;
		if (_prefix is { })
			name = name[_prefix.Length..];

		return new(name, _prefix);
	}

	bool IsFolderName(string name, int endIndex)
	{
		for (int i = endIndex; --i >= 0;)
		{
			if (char.IsLetterOrDigit(name[i]))
				continue;

			_folderSymbols ??= Settings.Default.GetData().FolderSymbols;
			if (_folderSymbols.IndexOf(name[i]) < 0)
				return false;
		}
		return true;
	}

	public override Panel CreatePanel()
	{
		return new KeysPanel(this);
	}

	protected override string PanelTitle()
	{
		var name = _colon is { } ? "Tree" : "Keys";
		var info = _prefix ?? _pattern ?? Database.Multiplexer.Configuration;
		return $"{name} {info}";
	}

	public override void EnterPanel(Panel panel)
	{
		base.EnterPanel(panel);

		if (_colon is { })
		{
			if (_prefix is { } && _prefix != _root)
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

		Dictionary<string, int>? folders = null;
		Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> lookup;
		if (_colon is { })
			lookup = (folders = []).GetAlternateLookup<ReadOnlySpan<char>>();

		foreach (RedisKey key in keys)
		{
			var name = ToFileName(key!);

			// folder?
			if (_colon is { })
			{
				int index = name.IndexOf(_colon);
				if (index >= 0 && IsFolderName(name, index))
				{
					var nameAndColon = name.AsSpan(0, index + _colon.Length);
					++CollectionsMarshal.GetValueRefOrAddDefault(lookup, nameAndColon, out _);
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
					Data = new Files.FileDataFolder(_prefix + nameAndColon),
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
		return new KeysExplorer(Database, _colon!, _root, prefix);
	}

	public override Explorer? ExploreParent(ExploreParentEventArgs args)
	{
		if (_colon is null || _prefix is null || _prefix == _root)
			return null;

		args.PostData = new Files.FileDataFolder(_prefix);

		var startIndex = _prefix.Length - _colon.Length - _colon.Length;
		if (startIndex < 0)
			return new KeysExplorer(Database, _colon, _root, null);

		var index = _prefix.LastIndexOf(_colon, startIndex);
		if (index < 0)
			return new KeysExplorer(Database, _colon, _root, null);

		return new KeysExplorer(Database, _colon, _root, _prefix[..(index + _colon.Length)]);
	}

	public override Explorer? ExploreRoot(ExploreRootEventArgs args)
	{
		if (_colon is null || _prefix is null || _prefix == _root)
			return null;

		return new KeysExplorer(Database, _colon, _root, null);
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
		var type = Database.KeyType(key);

		string? text = null;
		switch (type)
		{
			case RedisType.String:
				{
					text = Database.StringGet(key);
				}
				break;
			case RedisType.List:
				{
					var res = Database.ListRange(key);
					text = string.Join('\n', res.ToStringArray());
				}
				break;
			case RedisType.Set:
				{
					var res = Database.SetMembers(key);
					text = string.Join('\n', res.ToStringArray());
				}
				break;
		}

		if (text is null)
		{
			args.Result = JobResult.Ignore;
			return;
		}

		args.CanSet = true;
		args.UseText = text;
		if (type == RedisType.String)
			args.UseFileExtension = EditCommand.GetFileExtension(key.ToString());
	}

	public override void SetText(SetTextEventArgs args)
	{
		var key = args.File.DataKey().Key;
		var type = Database.KeyType(key);

		switch (type)
		{
			case RedisType.String:
				{
					Database.StringSet(key, args.Text);
				}
				break;
			case RedisType.List:
				{
					var lines = FarNet.Works.Kit.SplitLines(args.Text);
					Database.KeyDelete(key);
					Database.ListRightPush(key, lines.ToRedisValueArray());
				}
				break;
			case RedisType.Set:
				{
					var lines = FarNet.Works.Kit.SplitLines(args.Text);
					Database.KeyDelete(key);
					Database.SetAdd(key, lines.ToRedisValueArray());
				}
				break;
			default:
				{
					throw new ModuleException($"Unexpected Redis key type: {type}.");
				}
		}
	}
}
