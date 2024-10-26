using FarNet;
using GitKit.Extras;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit.Commands;

sealed class EditCommand : BaseCommand
{
	readonly string? _path;

	public EditCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_path = parameters.GetString(Parameter.Path, true);
	}

	string? InputPath()
	{
		var path = Far.Api.Input("Git file path", "GitFile", $"Edit in {Repository.Info.WorkingDirectory ?? Repository.Info.Path}");
		return string.IsNullOrEmpty(path) ? null : path;
	}

	public override void Invoke()
	{
		string? path = _path ?? InputPath();
		if (path is null)
			return;

		path = Lib.ResolveRepositoryItemPath(Repository, path);

		var editor = Far.Api.CreateEditor();
		editor.FileName = path;
		editor.Open();
	}
}
