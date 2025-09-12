using System.Text.Json;
using System.Text.Json.Nodes;

namespace FarNet.Json;

static class JsonNodeReader
{
	public static List<JsonNode?> ReadNodes(ref Utf8JsonReader reader)
	{
		List<JsonNode?> res = [];
		while (reader.Read())
			res.Add(ReadNode(ref reader));
		return res;
	}

	static JsonNode? ReadNode(ref Utf8JsonReader reader)
	{
		switch (reader.TokenType)
		{
			case JsonTokenType.String:
				return JsonValue.Create(reader.GetString()!);

			case JsonTokenType.Number:
				return JsonValue.Create(reader.GetDecimal()!);

			case JsonTokenType.False:
			case JsonTokenType.True:
				return JsonValue.Create(reader.GetBoolean()!);

			case JsonTokenType.Null:
				return null;

			case JsonTokenType.StartArray:
				return ReadArray(ref reader);

			case JsonTokenType.StartObject:
				return ReadObject(ref reader);

			default:
				throw new InvalidOperationException($"Unexpected token type: '{reader.TokenType}'.");
		}
	}

	static JsonArray ReadArray(ref Utf8JsonReader reader)
	{
		JsonArray res = [];
		while (true)
		{
			reader.Read();
			if (reader.TokenType == JsonTokenType.EndArray)
				break;

			res.Add(ReadNode(ref reader));
		}
		return res;
	}

	static JsonObject ReadObject(ref Utf8JsonReader reader)
	{
		JsonObject res = [];
		while (true)
		{
			reader.Read();
			if (reader.TokenType == JsonTokenType.EndObject)
				break;

			if (reader.TokenType != JsonTokenType.PropertyName)
				throw new InvalidOperationException("Expected property.");

			var name = reader.GetString()!;
			reader.Read();

			res.Add(name, ReadNode(ref reader));
		}
		return res;
	}
}
