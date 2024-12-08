using FarNet;
using GitKit.Extras;
using LibGit2Sharp;
using System.IO;
using System.Linq;
using System.Text;

namespace GitKit.Commands;

sealed class CommitCommand : BaseCommand
{
	readonly CommitOptions op = new();
	readonly string? _message;
	readonly bool _All;
	readonly char _CommentaryChar;

	public CommitCommand(CommandParameters parameters) : base(parameters)
	{
		_message = parameters.GetString(Parameter.Message);

		_All = parameters.GetBool(Parameter.All);

		op.AmendPreviousCommit = parameters.GetBool(Parameter.AmendPreviousCommit);
		op.AllowEmptyCommit = parameters.GetBool(Parameter.AllowEmptyCommit);

		var PrettifyMessage = parameters.GetBool(Parameter.PrettifyMessage);
		_CommentaryChar = parameters.GetValue<char>(Parameter.CommentaryChar);
		if (_CommentaryChar == 0)
		{
			op.PrettifyMessage = PrettifyMessage;
		}
		else
		{
			op.PrettifyMessage = true;
			op.CommentaryChar = _CommentaryChar;
		}
	}

	string GetMessage()
	{
		Commit? tip = Repository.Head.Tip;

		var message = string.Empty;
		if (op.AmendPreviousCommit && tip is not null)
			message = tip.Message;

		if (_CommentaryChar == 0 || tip is null)
			return message;

		var sb = new StringBuilder();
		sb.AppendLine(message.TrimEnd());
		sb.AppendLine();

		// warning about overriding remote commit
		if (op.AmendPreviousCommit && Repository.Head.TrackedBranch is not null && Repository.Head.TrackedBranch.Tip == tip)
		{
			sb.AppendLine($"{_CommentaryChar} WARNING:");
			sb.AppendLine($"{_CommentaryChar}\tThe remote commit will be amended.");
			sb.AppendLine();
		}

		// current changes
		{
			sb.AppendLine($"{_CommentaryChar} Changes to be committed:");

			var changes = Lib.CompareTree(
				Repository,
				tip.Tree,
				_All ? DiffTargets.Index | DiffTargets.WorkingDirectory : DiffTargets.Index);

			foreach (var change in changes)
				sb.AppendLine($"{_CommentaryChar}\t{change.Status}:\t{change.Path}");
		}

		// last changes to be amended
		if (op.AmendPreviousCommit)
		{
			sb.AppendLine();
			sb.AppendLine($"{_CommentaryChar} Changes to be amended:");

			TreeChanges changes = Lib.CompareTrees(Repository, tip.Parents.FirstOrDefault()?.Tree, tip.Tree);

			foreach (var change in changes)
				sb.AppendLine($"{_CommentaryChar}\t{change.Status}:\t{change.Path}");
		}

		return sb.ToString();
	}

	string EditMessage()
	{
		var message = GetMessage();

		var file = Path.Combine(Repository.Info.Path, "COMMIT_EDITMSG");
		File.WriteAllText(file, message);

		var editor = Far.Api.CreateEditor();
		editor.FileName = file;
		editor.CodePage = 65001;
		editor.DisableHistory = true;
		editor.Caret = new Point(0, 0);
		editor.Title = (op.AmendPreviousCommit ? "Amend commit" : "Commit") + $" on branch {Repository.Head.FriendlyName} -- empty message aborts the commit";
		editor.Open(OpenMode.Modal);

		message = File.ReadAllText(file);
		if (_CommentaryChar > 0)
		{
			op.PrettifyMessage = false;
			message = Commit.PrettifyMessage(message, _CommentaryChar);
		}

		return message;
	}

	public override void Invoke()
	{
		var message = _message ?? EditMessage();
		if (message.Length == 0)
		{
			Far.Api.UI.WriteLine("Aborting commit due to empty commit message.");
			return;
		}

		if (_All)
		{
			try
			{
				LibGit2Sharp.Commands.Stage(Repository, "*");
			}
			catch (LibGit2SharpException ex)
			{
				throw new ModuleException($"Cannot stage changes. Make sure directories are not repositories. Error: {ex.Message}", ex);
			}
		}

		var sig = Lib.BuildSignature(Repository);
		Repository.Commit(message, sig, sig, op);

		Host.UpdatePanels();
	}
}
