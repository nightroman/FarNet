﻿using FarNet;
using LibGit2Sharp;
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
		_message = parameters.GetString(Param.Message);

		_All = parameters.GetBool(Param.All);

		op.AmendPreviousCommit = parameters.GetBool(Param.AmendPreviousCommit);
		op.AllowEmptyCommit = parameters.GetBool(Param.AllowEmptyCommit);

		var PrettifyMessage = parameters.GetBool(Param.PrettifyMessage);
		_CommentaryChar = parameters.GetValue<char>(Param.CommentaryChar);
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
		using var repo = new Repository(GitDir);

		Commit? tip = repo.Head.Tip;

		var message = string.Empty;
		if (op.AmendPreviousCommit && tip is not null)
			message = tip.Message;

		if (_CommentaryChar == 0 || tip is null)
			return message;

		var sb = new StringBuilder();
		sb.AppendLine(message.TrimEnd());
		sb.AppendLine();

		// warning about overriding remote commit
		if (op.AmendPreviousCommit && repo.Head.TrackedBranch is not null && repo.Head.TrackedBranch.Tip == tip)
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
		if (op.AmendPreviousCommit)
		{
			sb.AppendLine();
			sb.AppendLine($"{_CommentaryChar} Changes to be amended:");

			TreeChanges changes = Lib.CompareTrees(repo, tip.Parents.FirstOrDefault()?.Tree, tip.Tree);

			foreach (var change in changes)
				sb.AppendLine($"{_CommentaryChar}\t{change.Status}:\t{change.Path}");
		}

		return sb.ToString();
	}

	string EditMessage()
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
		editor.Title = (op.AmendPreviousCommit ? "Amend commit" : "Commit") + $" on branch {repo.Head.FriendlyName} -- empty message aborts the commit";
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
		using var repo = new Repository(GitDir);

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
				LibGit2Sharp.Commands.Stage(repo, "*");
			}
			catch (LibGit2SharpException ex)
			{
				throw new ModuleException($"Cannot stage changes. Make sure directories are not repositories. Error: {ex.Message}", ex);
			}
		}

		var sig = Lib.BuildSignature(repo);
		repo.Commit(message, sig, sig, op);

		Host.UpdatePanels();
	}
}
