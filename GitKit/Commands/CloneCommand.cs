using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class CloneCommand : AnyCommand
{
	readonly string _url;
	readonly string _path;
	readonly CloneOptions _op;

	public CloneCommand(CommandParameters parameters)
	{
		_url = parameters.GetRequiredString(Param.Url);

		_path = parameters.GetPathOrCurrentDirectory(Param.Path);

		_op = new CloneOptions
		{
			IsBare = parameters.GetBool(Param.IsBare),
			RecurseSubmodules = parameters.GetBool(Param.RecurseSubmodules),
			FetchOptions =
			{
				Depth = parameters.GetValue<int>(Param.Depth)
			}
		};

		_op.FetchOptions.CredentialsProvider = Host.GetCredentialsHandler();

		if (parameters.GetBool(Param.NoCheckout))
			_op.Checkout = false;
	}

	public override void Invoke()
	{
		Repository.Clone(_url, _path, _op);
	}
}
