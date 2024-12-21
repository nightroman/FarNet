using FarNet;
using GitKit.Panels;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class CommitsCommand(CommandParameters parameters) : BaseCommand(parameters)
{
	string? _path = parameters.GetString(Param.Path, ParameterOptions.ExpandVariables);
	readonly bool _isGitPath = parameters.GetBool(Param.IsGitPath);

	public override void Invoke()
	{
		using var repo = new Repository(GitRoot);

		_path = GetGitPathOrPath(
			repo,
			_path,
			_isGitPath,
			returnNullIfRoot: true,
			validate: path => path == "?" ? Far.Api.FS.CursorPath ?? Far.Api.CurrentDirectory : path);

		var explorer = _path is null ?
			new CommitsExplorer(GitRoot, repo.Head.FriendlyName, false) :
			new CommitsExplorer(GitRoot, _path, true);

		explorer
			.CreatePanel()
			.Open();
	}
}
