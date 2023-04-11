using FarNet;
using LibGit2Sharp;
using System;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace GitKit;

[ModuleCommand(Name = "GitKit", Prefix = "gk", Id = "15a36561-bf47-47a5-ae43-9729eda272a3")]
public class Command : ModuleCommand
{
	DbConnectionStringBuilder _parameters = null!;
	Repository _repo = null!;

	public override void Invoke(object sender, ModuleCommandEventArgs e)
	{
		try
		{
			_parameters = Parameters.Parse(e.Command);

			//
			// new repository
			//

			var init = _parameters.GetValue("init");
			if (init is not null)
			{
				InitCommand(init);
				return;
			}

			var clone = _parameters.GetValue("clone");
			if (clone is not null)
			{
				CloneCommand(clone);
				return;
			}

			//
			// existing repository
			//

			var path = _parameters.GetValue("repo");
			path = Host.GetFullPath(path);

			_repo = new Repository(Lib.GetGitRoot(path));

			if (_parameters.Count == 0)
			{
				StatusCommand();
				return;
			}

			var panel = _parameters.GetValue("panel");
			if (panel is not null)
			{
				PanelCommand(panel);
				return;
			}

			var commit = _parameters.GetValue("commit");
			if (commit is not null)
			{
				CommitCommand(commit);
				return;
			}

			_parameters.AssertNone();
		}
		catch (ModuleException)
		{
			throw;
		}
		catch (LibGit2SharpException ex)
		{
			throw new ModuleException(ex.Message, ex);
		}
	}

	void InitCommand(string path)
	{
		path = Host.GetFullPath(path);

		var IsBare = _parameters.GetValue<bool>("IsBare");

		_parameters.AssertNone();
		Repository.Init(path, IsBare);
	}

	void CloneCommand(string url)
	{
		var Path = _parameters.GetValue("Path");
		Path = Host.GetFullPath(Path);

		var op = new CloneOptions();
		op.IsBare = _parameters.GetValue<bool>("IsBare");
		op.RecurseSubmodules = _parameters.GetValue<bool>("RecurseSubmodules");
		if (_parameters.GetValue<bool>("NoCheckout"))
			op.Checkout = false;

		_parameters.AssertNone();
		Repository.Clone(url, Path, op);
	}

	void StatusCommand()
	{
		// see TreeChanges.DebuggerDisplay
		var changes = _repo.Diff.Compare<TreeChanges>(_repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory);
		if (changes.Count > 0)
		{
			int n;

			Far.Api.UI.Write($"{changes.Count} ");

			n = changes.Added.Count();
			if (n > 0)
				Far.Api.UI.Write($"a{n} ");

			n = changes.Modified.Count();
			if (n > 0)
				Far.Api.UI.Write($"m{n} ");

			n = changes.Deleted.Count();
			if (n > 0)
				Far.Api.UI.Write($"d{n} ");

			n = changes.TypeChanged.Count();
			if (n > 0)
				Far.Api.UI.Write($"\u00B1{n} ");

			n = changes.Renamed.Count();
			if (n > 0)
				Far.Api.UI.Write($"r{n} ");

			n = changes.Copied.Count();
			if (n > 0)
				Far.Api.UI.Write($"c{n} ");
		}

		var commit = _repo.Head.Tip;
		Far.Api.UI.Write(commit.Sha[0..7], ConsoleColor.DarkYellow);
		Far.Api.UI.Write(" (");
		Far.Api.UI.Write("HEAD -> ", ConsoleColor.Cyan);

		bool comma = false;
		foreach (var branch in _repo.Branches.Where(x => x.Tip == commit && x.FriendlyName != "origin/HEAD"))
		{
			if (comma)
				Far.Api.UI.Write(", ");

			comma = true;
			Far.Api.UI.Write(branch.FriendlyName, branch.IsRemote ? ConsoleColor.Red : branch.IsCurrentRepositoryHead ? ConsoleColor.Green: ConsoleColor.Gray);
		}

		Far.Api.UI.Write($") {commit.MessageShort}");
		Far.Api.UI.WriteLine();
	}

	void PanelCommand(string panel)
	{
		switch (panel)
		{
			case "branches":
				_parameters.AssertNone();
				new BranchesExplorer(_repo).CreatePanel().Open();
				return;

			case "commits":
				_parameters.AssertNone();
				new CommitsExplorer(_repo, _repo.Head).CreatePanel().Open();
				return;

			case "changes":
				_parameters.AssertNone();
				TreeChanges changes() => _repo.Diff.Compare<TreeChanges>(_repo.Head.Tip.Tree, DiffTargets.Index | DiffTargets.WorkingDirectory);
				new ChangesExplorer(_repo, changes).CreatePanel().Open();
				return;

			default:
				throw new ModuleException($"Unknown panel `{panel}`.");
		}
	}

	void CommitCommand(string message)
	{
		var op = new CommitOptions();

		var All = _parameters.GetValue<bool>("All");

		op.AmendPreviousCommit = _parameters.GetValue<bool>("AmendPreviousCommit");
		op.AllowEmptyCommit = _parameters.GetValue<bool>("AllowEmptyCommit");

		var PrettifyMessage = _parameters.GetValue<bool>("PrettifyMessage");
		var CommentaryChar = _parameters.GetValue<char>("CommentaryChar");
		if (CommentaryChar == 0)
		{
			op.PrettifyMessage = PrettifyMessage;
		}
		else
		{
			op.PrettifyMessage = true;
			op.CommentaryChar = CommentaryChar;
		}

		_parameters.AssertNone();

		if (message == "#")
		{
			message = string.Empty;
			if (op.AmendPreviousCommit)
				message = _repo.Head.Tip.Message;

			if (CommentaryChar > 0)
			{
				var sb = new StringBuilder();
				sb.AppendLine(message.TrimEnd());
				sb.AppendLine();

				sb.AppendLine($"{CommentaryChar} Changes to be committed:");

				var changes = _repo.Diff.Compare<TreeChanges>(
					_repo.Head.Tip.Tree,
					All ? (DiffTargets.Index | DiffTargets.WorkingDirectory) : DiffTargets.Index);

				foreach(var change in changes)
					sb.AppendLine($"{CommentaryChar}\t{change.Status}:\t{change.Path}");

				message = sb.ToString();
			}

			var file = Path.Combine(_repo.Info.Path, "COMMIT_EDITMSG");
			File.WriteAllText(file, message);

			var editor = Far.Api.CreateEditor();
			editor.FileName = file;
			editor.CodePage = 65001;
			editor.DisableHistory = true;
			editor.Caret = new Point(0, 0);
			editor.Title = (op.AmendPreviousCommit ? "Amend commit" : "Commit") + $" on branch {_repo.Head.FriendlyName} -- empty message aborts the commit";
			editor.Open(OpenMode.Modal);

			message = File.ReadAllText(file);
			if (CommentaryChar > 0)
			{
				op.PrettifyMessage = false;
				message = Commit.PrettifyMessage(message, CommentaryChar);
			}

			if (message.Length == 0)
			{
				Far.Api.UI.WriteLine("Aborting commit due to empty commit message.");
				return;
			}
		}

		if (All)
			Commands.Stage(_repo, "*");

		var sig = _repo.Config.BuildSignature(DateTimeOffset.UtcNow);
		_repo.Commit(message, sig, sig, op);
	}
}
