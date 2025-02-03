using FarNet;
using FarNet.Redis;
using FarNet.Redis.Commands;
using StackExchange.Redis;
using System.Text;

namespace RedisKit;

static class About
{
	static ModuleException CannotConvertToText => new("Cannot convert data to text.");

	public static string ExportJson(IDatabase db, IEnumerable<RedisKey> keys)
	{
		using var stream = new MemoryStream();
		var command = new ExportJson
		{
			Database = db,
			Stream = stream,
			Keys = keys,
			FormatAsObjects = true,
			TimeToLive = TimeSpan.Zero,
		};
		command.Invoke();
		return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
	}

	public static void ImportJson(IDatabase db, string json)
	{
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
		var command = new ImportJson
		{
			Database = db,
			Stream = stream,
		};
		command.Invoke();
	}

	public static string StringToText(IDatabase db, RedisKey key)
	{
		var res = db.StringGet(key);
		var blob = AboutRedis.GetBlobOrText(res);
		if (blob is string text)
			return text;

		throw CannotConvertToText;
	}

	public static string HashToText(IDatabase db, RedisKey key)
	{
		var res = db.HashGetAll(key);
		var text = AboutRedis.HashToText(res);
		if (text is { })
			return text;

		throw CannotConvertToText;
	}

	public static string ListToText(IDatabase db, RedisKey key)
	{
		var res = db.ListRange(key);
		var text = AboutRedis.ValuesToText(res);
		if (text is { })
			return text;

		throw CannotConvertToText;
	}

	public static string SetToText(IDatabase db, RedisKey key)
	{
		var res = db.SetMembers(key);
		var text = AboutRedis.ValuesToText(res);
		if (text is { })
			return text;

		throw CannotConvertToText;
	}

	public static HashEntry[] TextToHash(IList<string> lines)
	{
		try
		{
			return AboutRedis.TextToHash(lines);
		}
		catch (AboutRedis.Exception ex)
		{
			throw new ModuleException(ex.Message);
		}
	}
}
