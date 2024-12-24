using FarNet;
using GitKit.About;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class EditCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	readonly string? _path = parameters.GetString(Param.Path, ParameterOptions.ExpandVariables);

	string? InputPath(Repository repo)
	{
		var path = Far.Api.Input("Git file path", "GitFile", $"Edit in {repo.Info.WorkingDirectory ?? repo.Info.Path}");
		return string.IsNullOrEmpty(path) ? null : path;
	}

	public override void Invoke()
	{
		using var repo = new Repository(GitDir);

		string? path = _path ?? InputPath(repo);
		if (path is null)
			return;

		path = Lib.ResolveRepositoryItemPath(repo, path);

		var editor = Far.Api.CreateEditor();
		editor.FileName = path;
		editor.Open();
	}
}
