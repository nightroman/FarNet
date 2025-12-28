using FarNet;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace RedisKit.Commands;

sealed class JsonCommand : BaseCommand
{
	readonly string _mask;

	public JsonCommand(CommandParameters parameters) : base(parameters)
	{
		_mask = parameters.GetRequiredString(ParamMask);
	}

	public override void Invoke()
	{
		IEnumerable<RedisKey> keys;
		if (_mask.Contains('[') || _mask.Contains(']'))
		{
			keys = DB.Keys(Database, _mask);
		}
		else if (_mask.Contains('*') || _mask.Contains('?'))
		{
			keys = DB.Keys(Database, _mask.Replace("\\", "\\\\"));
		}
		else
		{
			keys = [_mask];
		}

		EditTextArgs args = new()
		{
			Title = _mask,
			Text = About.ExportJson(Database, keys),
			Extension = "json",
			EditorSaving = (s, e) => EditorSaving((IEditor)s!)
		};
		Far.Api.AnyEditor.EditTextAsync(args);
	}

	void EditorSaving(IEditor editor)
	{
		About.ImportJson(Database, editor.GetText());
	}
}
