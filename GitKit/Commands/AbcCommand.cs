using FarNet;

namespace GitKit.Commands;

abstract class AbcCommand : Subcommand
{
	public const string ParamAll = "All";
	public const string ParamAllowEmptyCommit = "AllowEmptyCommit";
	public const string ParamAmendPreviousCommit = "AmendPreviousCommit";
	public const string ParamBranch = "Branch";
	public const string ParamCommentaryChar = "CommentaryChar";
	public const string ParamDepth = "Depth";
	public const string ParamIsBare = "IsBare";
	public const string ParamIsGitPath = "IsGitPath";
	public const string ParamKind = "Kind";
	public const string ParamMessage = "Message";
	public const string ParamNoCheckout = "NoCheckout";
	public const string ParamPath = "Path";
	public const string ParamPrettifyMessage = "PrettifyMessage";
	public const string ParamRecurseSubmodules = "RecurseSubmodules";
	public const string ParamRepo = "Repo";
	public const string ParamUrl = "Url";
}
