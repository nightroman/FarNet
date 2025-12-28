using FarNet;
using FarNet.Redis;
using StackExchange.Redis;

namespace RedisKit.Commands;

sealed class EditCommand : BaseCommand
{
	readonly RedisKey _key;

	public EditCommand(CommandParameters parameters) : base(parameters)
	{
		_key = parameters.GetRequiredString(ParamKey);
	}

	public override void Invoke()
	{
		var type = Database.KeyType(_key);

		EditTextArgs args = new()
		{
			Title = $"{type} {_key}",
			EditorOpened = (s, e) =>
			{
				var editor = (IEditor)s!;
				editor.Saving += (s, e) => EditorSaving(editor, type);
			}
		};

		switch (type)
		{
			case RedisType.None:
				{
					args.Text = string.Empty;
					args.Extension = GetFileExtension(_key.ToString());
				}
				break;
			case RedisType.String:
				{
					args.Text = About.StringToText(Database, _key);
					args.Extension = GetFileExtension(_key.ToString());
				}
				break;
			case RedisType.Hash:
				{
					args.Text = About.HashToText(Database, _key);
				}
				break;
			case RedisType.List:
				{
					args.Text = About.ListToText(Database, _key);
				}
				break;
			case RedisType.Set:
				{
					args.Text = About.SetToText(Database, _key);
				}
				break;
			default:
				{
					throw new ModuleException($"Not supported Redis type: {type}.");
				}
		}

		Far.Api.AnyEditor.EditTextAsync(args);
	}

	void EditorSaving(IEditor editor, RedisType type)
	{
		switch (type)
		{
			case RedisType.None:
			case RedisType.String:
				{
					var text = editor.GetText();

					Database.KeyDelete(_key);
					Database.StringSet(_key, text);
				}
				break;
			case RedisType.Hash:
				{
					var items = About.TextToHash(editor.Strings);

					Database.KeyDelete(_key);
					Database.HashSet(_key, items);
				}
				break;
			case RedisType.List:
				{
					var items = editor.Strings.ToArray().ToRedisValueArray();

					Database.KeyDelete(_key);
					Database.ListRightPush(_key, items);
				}
				break;
			case RedisType.Set:
				{
					var items = editor.Strings.ToArray().ToRedisValueArray();

					Database.KeyDelete(_key);
					Database.SetAdd(_key, items);
				}
				break;
		}
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
