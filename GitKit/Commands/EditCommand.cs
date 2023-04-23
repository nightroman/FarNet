using FarNet;
using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class EditCommand : BaseCommand
{
	readonly string _path;

	public EditCommand(Repository repo, DbConnectionStringBuilder parameters) : base(repo)
	{
		_path = parameters.GetValue("Path") ?? throw new ModuleException("Required parameter 'Path'.");
	}

	public override void Invoke()
	{
		string path = Lib.ResolveRepositoryItemPath(_repo, _path);

		var editor = Far.Api.CreateEditor();
		editor.FileName = path;
		editor.Open();
	}
}
