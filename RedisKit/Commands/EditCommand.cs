using FarNet;
using StackExchange.Redis;
using System;
using System.Data.Common;
using System.IO;

namespace RedisKit;

sealed class EditCommand(DbConnectionStringBuilder parameters) : BaseCommand(parameters)
{
    readonly string _key = parameters.GetRequiredString(Host.Param.Key);

	public override void Invoke()
	{
		RedisKey key = _key;

		var type = Database.KeyType(key);
		if (type != RedisType.String && type != RedisType.None)
			throw new InvalidOperationException($"Expected 'String'. The actual key is '{type}'.");

		var text = (string?)Database.StringGet(key);
		EditTextArgs args = new()
		{
			Text = text,
			Title = _key,
			Extension = GetFileExtension(_key),
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
