using FarNet;
using StackExchange.Redis;

namespace RedisKit.Commands;

sealed class EditCommand : BaseCommand
{
	readonly RedisKey _key;

	public EditCommand(CommandParameters parameters) : base(parameters)
	{
		_key = parameters.GetRequiredString(Param.Key);
	}

	public override void Invoke()
	{
		var type = Database.KeyType(_key);

		string? text = null;
		switch (type)
		{
			case RedisType.None:
				{
					text = string.Empty;
				}
				break;
			case RedisType.String:
				{
					text = Database.StringGet(_key);
				}
				break;
			case RedisType.List:
				{
					var res = Database.ListRange(_key);
					text = string.Join('\n', res.ToStringArray());
				}
				break;
			case RedisType.Set:
				{
					var res = Database.SetMembers(_key);
					text = string.Join('\n', res.ToStringArray());
				}
				break;
		}

		if (text is null)
			throw new ModuleException($"Not supported Redis key type: {type}.");

		EditTextArgs args = new()
		{
			Text = text,
			Title = _key,
			EditorSaving = (s, e) => EditorSaving((IEditor)s!)
		};

		if (type == RedisType.String)
			args.Extension = GetFileExtension(_key.ToString());

		Far.Api.AnyEditor.EditTextAsync(args);
	}

	void EditorSaving(IEditor editor)
	{
		var type = Database.KeyType(_key);

		switch (type)
		{
			case RedisType.None:
			case RedisType.String:
				{
					Database.StringSet(_key, editor.GetText());
				}
				return;
			case RedisType.List:
				{
					var lines = editor.Strings.ToArray();
					Database.KeyDelete(_key);
					Database.ListRightPush(_key, lines.ToRedisValueArray());
				}
				return;
			case RedisType.Set:
				{
					var lines = editor.Strings.ToArray();
					Database.KeyDelete(_key);
					Database.SetAdd(_key, lines.ToRedisValueArray());
				}
				return;
		}

		throw new ModuleException($"Not supported Redis key type: {type}.");
	}

	public static string? GetFileExtension(string key)
	{
		var ext = Path.GetExtension(key);
		if (string.IsNullOrEmpty(ext))
			return null;

		for (int i = 1; i < ext.Length; i++)
		{
			if (!char.IsLetterOrDigit(ext, i))
				return null;
		}

		return ext;
	}
}
