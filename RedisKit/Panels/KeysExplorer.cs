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

    public KeysExplorer(IDatabase repository, string? mask) : base(repository, MyTypeId)
	{
		CanCloneFile = true;
		CanCreateFile = true;
		CanDeleteFiles = true;
		CanRenameFile = true;
		CanGetContent = true;
		CanSetText = true;

		if (mask is { })
			_pattern = mask.Contains('[') || mask.Contains(']') ? mask : mask.Replace("\\", "\\\\");
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

		foreach (RedisKey key in keys)
		{
			var file = new SetFile
            {
                Name = (string)key!,
                Data = key,
            };

			var type = Database.KeyType(key);
			switch(type)
			{
                case RedisType.Set:
                    file.Owner = "S";
                    break;
                case RedisType.List:
                    file.Owner = "L";
                    break;
                case RedisType.Hash:
                    file.Owner = "H";
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
		//CloneBranch(args, false);
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
		//CloneBranch(args, true);
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
        var newName = (string)args.Data!;
        Database.KeyRename(key, newName);
        args.PostName = newName;
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
    }

    public override void SetText(SetTextEventArgs args)
    {
        var key = (RedisKey)args.File.Data!;
		Database.StringSet(key, args.Text);
    }
}
