using FarNet;
using GitKit.Panels;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class BlameCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	const int AuthorNameMax = 15;
	string? _path = parameters.GetString(ParamPath, ParameterOptions.ExpandVariables);
	readonly bool _isGitPath = parameters.GetBool(ParamIsGitPath);

	public override void Invoke()
	{
		using var repo = new Repository(GitDir);

		_path = GetGitPathOrPath(
			repo,
			_path,
			_isGitPath,
			path => path ?? Far.Api.FS.CursorFile?.FullName);

		if (_path is null)
			return;

		// get lines from the blob to ensure the same content as used by blame
		var lines = new List<string>();
		{
			var blob = repo.Head.Tip?.Tree[_path]?.Target as Blob
				?? throw new ModuleException($"Cannot find '{_path}' in the tree.");

			using var stream = blob.GetContentStream();
			using var reader = new StreamReader(stream);
			string? line;
			while ((line = reader.ReadLine()) is not null)
				lines.Add(line);
		}

		// blame
		var hunks = repo.Blame(_path);

		// write annotated temp file
		var settings = Settings.Default.GetData();
		var white = string.Empty.PadRight(settings.ShaPrefixLength + AuthorNameMax + 12);
		var tempFile = FarNet.Works.Kit.TempFileName("txt");
		{
			using var writer = new StreamWriter(tempFile);
			foreach (var hunk in hunks)
			{
				var sha = hunk.FinalCommit.Sha[0..settings.ShaPrefixLength];
				var date = hunk.FinalCommit.Author.When.ToString("yyyy-MM-dd");
				var author = hunk.FinalCommit.Author.Name;
				if (author.Length > AuthorNameMax)
					author = author[0..AuthorNameMax];

				for (int n = 0; n < hunk.LineCount; ++n)
				{
					int i = hunk.FinalStartLineNumber + n;
					if (i >= lines.Count)
						break;

					if (n == 0)
						writer.Write($"{sha} {date} {author,-AuthorNameMax}");
					else
						writer.Write(white);

					writer.WriteLine($" {i + 1,4} {lines[i]}");
				}
			}
		}

		// edit temp file
		var editor = Far.Api.CreateEditor();
		editor.FileName = tempFile;
		editor.CodePage = 65001;
		editor.Title = _path;
		editor.DeleteSource = DeleteSource.File;
		editor.DisableHistory = true;
		editor.IsLocked = true;

		editor.KeyDown += Editor_KeyDown;

		editor.Open();
	}

	void Editor_KeyDown(object? sender, KeyEventArgs e)
	{
		switch (e.Key.VirtualKeyCode)
		{
			case KeyCode.Enter when e.Key.Is():
				OpenChangesPanel((IEditor)sender!);
				e.Ignore = true;
				break;
			case KeyCode.L when e.Key.IsCtrl():
				e.Ignore = true;
				break;
		}
	}

	void OpenChangesPanel(IEditor editor)
	{
		using var repo = new Repository(GitDir);

		string? sha = null;
		for (int i = editor.Caret.Y; i >= 0 && sha is null; --i)
		{
			var text = editor[i].Text;
			for (int c = 0; c < text.Length; ++c)
			{
				if (char.IsWhiteSpace(text[c]))
				{
					if (c > 0)
						sha = text[0..c];
					break;
				}
			}
		}

		if (sha is null)
			return;

		var newCommit = repo.Lookup<Commit>(sha);
		if (newCommit is null)
			return;

		var oldCommit = newCommit.Parents.FirstOrDefault();

		// open changes panel
		var explorer = new ChangesExplorer(GitDir, new()
		{
			Kind = ChangesExplorer.Kind.CommitsRange,
			NewCommitSha = newCommit.Sha,
			OldCommitSha = oldCommit?.Sha,
			IsSingleCommit = true,
			ItemPath = _path,
		});
		var panel = explorer.CreatePanel();
		panel.Open();
	}
}
