﻿namespace GitKit.Commands;

abstract class AbcCommand
{
	protected static class Param
	{
		public const string All = "All";
		public const string AllowEmptyCommit = "AllowEmptyCommit";
		public const string AmendPreviousCommit = "AmendPreviousCommit";
		public const string Branch = "Branch";
		public const string CommentaryChar = "CommentaryChar";
		public const string Depth = "Depth";
		public const string IsBare = "IsBare";
		public const string IsGitPath = "IsGitPath";
		public const string Kind = "Kind";
		public const string Message = "Message";
		public const string Name = "Name";
		public const string NoCheckout = "NoCheckout";
		public const string Path = "Path";
		public const string PrettifyMessage = "PrettifyMessage";
		public const string RecurseSubmodules = "RecurseSubmodules";
		public const string Repo = "Repo";
		public const string Url = "Url";
	}

	public abstract void Invoke();
}
