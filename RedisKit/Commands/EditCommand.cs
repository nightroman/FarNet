using FarNet;
using StackExchange.Redis;
using System.IO;

namespace RedisKit.Commands;

sealed class EditCommand : BaseCommand
{
	readonly RedisKey _key;

	public EditCommand(CommandParameters parameters) : base(parameters)
	{
		_key = GetRequiredRedisKeyOfType(parameters, RedisType.String);
	}

	public override void Invoke()
	{
		var text = (string?)Database.StringGet(_key);
		EditTextArgs args = new()
		{
			Text = text,
			Title = _key,
			Extension = GetFileExtension(_key.ToString()),
			EditorSaving = (s, e) =>
			{
				var text = ((IEditor)s!).GetText();
				Database.StringSet(_key, text);
			}
		};
		Far.Api.AnyEditor.EditTextAsync(args);
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
