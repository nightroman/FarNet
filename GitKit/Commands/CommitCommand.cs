using FarNet;
using LibGit2Sharp;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace GitKit;

sealed class CommitCommand : BaseCommand
{
	readonly CommitOptions op = new();
	readonly string? _message;
	readonly bool _All;
	readonly char _CommentaryChar;

	public CommitCommand(Repository repo, DbConnectionStringBuilder parameters) : base(repo)
	{
		_message = parameters.GetValue("Message");

		_All = parameters.GetValue<bool>("All");

		op.AmendPreviousCommit = parameters.GetValue<bool>("AmendPreviousCommit");
		op.AllowEmptyCommit = parameters.GetValue<bool>("AllowEmptyCommit");

		var PrettifyMessage = parameters.GetValue<bool>("PrettifyMessage");
		_CommentaryChar = parameters.GetValue<char>("CommentaryChar");
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
				_All ? (DiffTargets.Index | DiffTargets.WorkingDirectory) : DiffTargets.Index);

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
			Commands.Stage(Repository, "*");

		var sig = Lib.BuildSignature(Repository);
		Repository.Commit(message, sig, sig, op);

		Host.UpdatePanels();
	}
}
