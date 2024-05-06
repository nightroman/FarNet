using FarNet;
using StackExchange.Redis;
using System;
using System.Data.Common;

namespace RedisKit;

sealed class EditCommand(DbConnectionStringBuilder parameters) : BaseCommand(parameters)
{
    readonly RedisKey _key = parameters.GetRequiredString(Host.Param.Key);

	public override void Invoke()
	{
		var type = Database.KeyType(_key);
		if (type != RedisType.String && type != RedisType.None)
			throw new InvalidOperationException($"Cannot open 'String', the key is '{type}'.");

		var text = (string?)Database.StringGet(_key);
		EditTextArgs args = new()
		{
			Text = text,
			Title = _key,
			EditorOpened = (s, e) =>
			{
				((IEditor)s!).Saving += (s, e) =>
				{
					var text = ((IEditor)s!).GetText();
					Database.StringSet(_key, text);
				};
			}
		};
		Far.Api.AnyEditor.EditTextAsync(args);
	}
}
