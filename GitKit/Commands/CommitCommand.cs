using FarNet;
using LibGit2Sharp;
using System.Text;

namespace GitKit.Commands;

sealed class CommitCommand : BaseCommand
{
	readonly CommitOptions _op = new();
	readonly string? _message;
	readonly bool _All;
	readonly char _CommentaryChar;

	public CommitCommand(CommandParameters parameters) : base(parameters)
	{
		_message = parameters.GetString(Param.Message);

		_All = parameters.GetBool(Param.All);

		_op.AmendPreviousCommit = parameters.GetBool(Param.AmendPreviousCommit);
		_op.AllowEmptyCommit = parameters.GetBool(Param.AllowEmptyCommit);

		var PrettifyMessage = parameters.GetBool(Param.PrettifyMessage);
		_CommentaryChar = parameters.GetValue<char>(Param.CommentaryChar);
		if (_CommentaryChar == 0)
		{
			_op.PrettifyMessage = PrettifyMessage;
		}
		else
		{
			_op.PrettifyMessage = true;
			_op.CommentaryChar = _CommentaryChar;
		}
	}

	string GetMessage()
	{
		using var repo = new Repository(GitDir);

		Commit? tip = repo.Head.Tip;

		var message = string.Empty;
		if (_op.AmendPreviousCommit && tip is not null)
			message = tip.Message;

		if (_CommentaryChar == 0 || tip is null)
			return message;

		var sb = new StringBuilder();
		sb.AppendLine(message.TrimEnd());
		sb.AppendLine();

		// warning about overriding remote commit
		if (_op.AmendPreviousCommit && repo.Head.TrackedBranch is not null && repo.Head.TrackedBranch.Tip == tip)
		{
			sb.AppendLine($"{_CommentaryChar} WARNING:");
			sb.AppendLine($"{_CommentaryChar}\tThe remote commit will be amended.");
			sb.AppendLine();
		}

		// current changes
		{
			sb.AppendLine($"{_CommentaryChar} Changes to be committed:");

			var changes = Lib.CompareTree(
				repo,
				tip.Tree,
				_All ? DiffTargets.Index | DiffTargets.WorkingDirectory : DiffTargets.Index);

			foreach (var change in changes)
				sb.AppendLine($"{_CommentaryChar}\t{change.Status}:\t{change.Path}");
		}

		// last changes to be amended
		if (_op.AmendPreviousCommit)
		{
			sb.AppendLine();
			sb.AppendLine($"{_CommentaryChar} Changes to be amended:");

			TreeChanges changes = Lib.CompareTrees(repo, tip.Parents.FirstOrDefault()?.Tree, tip.Tree);

			foreach (var change in changes)
				sb.AppendLine($"{_CommentaryChar}\t{change.Status}:\t{change.Path}");
		}

		return sb.ToString();
	}

	void EditMessageThen(Action<string> then)
	{
		using var repo = new Repository(GitDir);

		var message = GetMessage();

		var file = Path.Join(repo.Info.Path, "COMMIT_EDITMSG");
		File.WriteAllText(file, message);

		var editor = Far.Api.CreateEditor();
		editor.FileName = file;
		editor.CodePage = 65001;
		editor.DisableHistory = true;
		editor.Caret = new Point(0, 0);
		editor.Title = (_op.AmendPreviousCommit ? "Amend commit" : "Commit") + $" on branch {repo.Head.FriendlyName} -- empty message aborts the commit";

		editor.Closed += (s, e) =>
		{
			message = File.ReadAllText(file);
			File.Delete(file);

			if (_CommentaryChar > 0)
			{
				_op.PrettifyMessage = false;
				message = LibGit2Sharp.Commit.PrettifyMessage(message, _CommentaryChar);
			}

			then(message);
		};

		editor.Open();
	}

	void Commit(string message)
	{
		if (string.IsNullOrWhiteSpace(message))
		{
			Far.Api.UI.WriteLine("Aborting commit due to empty commit message.");
			return;
		}

		using var repo = new Repository(GitDir);
		if (_All)
		{
			try
			{
				LibGit2Sharp.Commands.Stage(repo, "*");
			}
			catch (LibGit2SharpException ex)
			{
				throw new ModuleException($"Cannot stage changes. Make sure directories are not repositories. Error: {ex.Message}", ex);
			}
		}

		var sig = Lib.BuildSignature(repo);
		repo.Commit(message, sig, sig, _op);

		Host.UpdatePanels();
	}

	public override void Invoke()
	{
		if (string.IsNullOrWhiteSpace(_message))
			EditMessageThen(Commit);
		else
			Commit(_message);
	}
}
