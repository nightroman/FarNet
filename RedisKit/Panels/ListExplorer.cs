using FarNet;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RedisKit;

class ListExplorer : BaseExplorer
{
	public static Guid MyTypeId = new("be46affb-dd5c-436b-99c3-197dfd6e9d1f");
    readonly RedisKey _key;

    public ListExplorer(IDatabase repository, RedisKey key) : base(repository, MyTypeId)
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
		foreach (RedisValue item in list)
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
