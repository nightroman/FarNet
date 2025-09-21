using FarNet;
using LibGit2Sharp;
using System.Text;
using System.Text.RegularExpressions;

namespace GitKit.Commands;

sealed partial class ConfigCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	// "core.repositoryformatversion"
	const int KeyColumnWidth = 28;

	[GeneratedRegex(@"^\s*(\w+)\s+(.+?)\s*=\s*(.*?)\s*$")]
	private static partial Regex ConfigLineRegex();

	public override void Invoke()
	{
		using var repo = new Repository(GitDir);

		Far.Api.AnyEditor.EditTextAsync(new()
		{
			Text = GetConfigText(repo),
			Title = $"Config {repo.Info.WorkingDirectory}",
			Extension = "txt",
			IsLocked = true,
			EditorOpened = (s, e) => Opened((IEditor)s!),
		});
	}

	static string GetConfigText(Repository repo)
	{
		var config = repo.Config.ToList();

		int padLevel = config.Max(x => x.Level.ToString().Length) + 1;

		var sb = new StringBuilder();
		foreach (var item in config)
		{
			sb.Append(item.Level.ToString().PadRight(padLevel));
			sb.Append(item.Key.PadRight(KeyColumnWidth));
			sb.Append(" = ");
			sb.AppendLine(item.Value);
		}

		return sb.ToString();
	}

	void Opened(IEditor editor)
	{
		editor.KeyDown += Editor_KeyDown;
	}

	void Editor_KeyDown(object? sender, KeyEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.Delete when e.Key.Is():
				DeleteConfigLine((IEditor)sender!);
				e.Ignore = true;
				break;
			case KeyCode.Enter when e.Key.Is():
				EditConfigLine((IEditor)sender!);
				e.Ignore = true;
				break;
			case KeyCode.Insert when e.Key.Is():
				AddConfigLine((IEditor)sender!);
				e.Ignore = true;
				break;
			case KeyCode.L when e.Key.IsCtrl():
				e.Ignore = true;
				break;
		}
	}

	void AddConfigLine(IEditor editor)
	{
		var text = editor.Line.Text;
		var match = ConfigLineRegex().Match(text);
		text = match.Success ? $"{match.Groups[1]} {match.Groups[2]} = {match.Groups[3]}" : string.Empty;

		text = Far.Api.Input("New config entry", null, Host.MyName, text);
		if (text is null)
			return;

		match = ConfigLineRegex().Match(text);
		if (!match.Success)
			throw new ModuleException("Expected format: {level} {key} = {value}");

		if (!Enum.TryParse<ConfigurationLevel>(match.Groups[1].Value, true, out var level))
			throw new ModuleException($"Invalid level. Valid values: {string.Join(',', Enum.GetNames<ConfigurationLevel>().Select(x => x.ToString()))}");

		using var repo = new Repository(GitDir);

		var key = match.Groups[2].Value;
		if (repo.Config.Get<string>(key, level) is { })
			throw new ModuleException($"{level} {key} already exists.");

		var value = match.Groups[3].Value;

		Update(repo, editor, () => repo.Config.Add(key, value, level));
	}

	void DeleteConfigLine(IEditor editor)
	{
		var text = editor.Line.Text;
		var match = ConfigLineRegex().Match(text);
		if (!match.Success)
			return;

		var level = Enum.Parse<ConfigurationLevel>(match.Groups[1].Value);
		var key = match.Groups[2].Value;

		if (0 != Far.Api.Message($"Delete {level} {key}?", Host.MyName, MessageOptions.YesNo))
			return;

		using var repo = new Repository(GitDir);

		Update(repo, editor, () => repo.Config.Unset(key, level));
	}

	void EditConfigLine(IEditor editor)
	{
		var text = editor.Line.Text;
		var match = ConfigLineRegex().Match(text);
		if (!match.Success)
			return;

		var level = Enum.Parse<ConfigurationLevel>(match.Groups[1].Value);
		var key = match.Groups[2].Value;
		var value1 = match.Groups[3].Value;

		var value2 = Far.Api.Input(key, null, $"{level} config", value1);
		if (value2 is null || value2 == value1)
			return;

		using var repo = new Repository(GitDir);

		Update(repo, editor, () => repo.Config.Set(key, value2, level));
	}

	static void Update(Repository repo, IEditor editor, Action action)
	{
		editor.IsLocked = false;
		try
		{
			try { action(); }
			catch (Exception ex) { throw new ModuleException(ex.Message); }

			editor.SetText(GetConfigText(repo));
			editor.Save();
		}
		finally
		{
			editor.IsLocked = true;
		}
	}
}
