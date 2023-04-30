using FarNet;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class EditCommand : BaseCommand
{
	readonly string? _path;

	public EditCommand(DbConnectionStringBuilder parameters) : base(parameters)
	{
		_path = parameters.GetValue("Path");
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
