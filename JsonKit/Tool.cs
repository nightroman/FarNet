using FarNet;
using JsonKit.Panels;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JsonKit;

[ModuleTool(Name = Host.MyName, Options = ModuleToolOptions.Panels | ModuleToolOptions.Editor | ModuleToolOptions.Viewer, Id = "28541849-d26b-4456-8cdd-e14a2dfe9ee1")]
public class Tool : ModuleTool
{
	public override void Invoke(object sender, ModuleToolEventArgs e)
	{
		IMenu menu = Far.Api.CreateMenu();
		menu.Title = Host.MyName;
		menu.HelpTopic = GetHelpTopic("menu");

		if (e.From == ModuleToolOptions.Panels && Far.Api.Panel is AbcPanel panel)
		{
			panel.AddMenu(menu);
		}

		if (!Far.Api.Window.IsModal)
		{
			menu.Add("Open from &clipboard", (s, e) => OpenFromClipboard());
		}

		menu.Add("&Help", (s, e) => Host.Instance.ShowHelpTopic(string.Empty));

		menu.Show();
	}

	static void OpenFromClipboard()
	{
		try
		{
			var json = Far.Api.PasteFromClipboard();
			json = ResolveClipboardJson(json, out var filePath);

			var node = JsonNode.Parse(json, documentOptions: new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });

			AbcExplorer explorer = node switch
			{
				JsonArray jsonArray => new ArrayExplorer(jsonArray, new() { FilePath = filePath }),
				JsonObject jsonObject => new ObjectExplorer(jsonObject, new() { FilePath = filePath }),
				_ => throw new ModuleException("Unexpected node type.")
			};

			explorer.CreatePanel().Open();
		}
		catch (Exception ex)
		{
			throw new ModuleException($"""
			Open from clipboard expects JSON array or object or a file path like "*.json".
			Error: {ex.Message}
			""");
		}
	}

	static string ResolveClipboardJson(string json, out string? filePath)
	{
		int index1 = 0;
		while (index1 < json.Length && char.IsWhiteSpace(json[index1]))
			index1++;

		int index2 = json.Length - 1;
		while (index2 >= index1 && char.IsWhiteSpace(json[index2]))
			index2--;

		if (index2 >= index1 + 4 && MemoryExtensions.Equals(json.AsSpan(index2 - 4, 5), ".json", StringComparison.OrdinalIgnoreCase))
		{
			var path = Far.Api.GetFullPath(json[index1..(index2 + 1)]);
			if (File.Exists(path))
			{
				filePath = path;
				return File.ReadAllText(path);
			}
		}

		filePath = null;
		return json;
	}
}
