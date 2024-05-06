using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RedisKit;

class SetExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("75bbcfef-c464-4c80-a602-83b15bf404f9");
    readonly RedisKey _key;

    public SetExplorer(IDatabase repository, RedisKey key) : base(repository, MyTypeId)
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
		return $"Set {_key}";
	}

	public override Panel CreatePanel()
	{
		return new SetPanel(this);
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
	}

	public override void CreateFile(CreateFileEventArgs args)
	{
	}

	public override void DeleteFiles(DeleteFilesEventArgs args)
	{
    }

    public override void RenameFile(RenameFileEventArgs args)
	{
    }

    public override void GetContent(GetContentEventArgs args)
    {
    }

    public override void SetText(SetTextEventArgs args)
    {
    }
}
