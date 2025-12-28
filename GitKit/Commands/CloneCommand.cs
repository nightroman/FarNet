using FarNet;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class CloneCommand : AbcCommand
{
	readonly string _url;
	readonly string _path;
	readonly CloneOptions _op;

	public CloneCommand(CommandParameters parameters)
	{
		_url = parameters.GetRequiredString(ParamUrl);

		_path = parameters.GetPathOrCurrentDirectory(ParamPath);

		_op = new CloneOptions
		{
			IsBare = parameters.GetBool(ParamIsBare),
			RecurseSubmodules = parameters.GetBool(ParamRecurseSubmodules),
			FetchOptions =
			{
				Depth = parameters.GetValue<int>(ParamDepth)
			}
		};

		_op.FetchOptions.CredentialsProvider = Host.GetCredentialsHandler();

		if (parameters.GetBool(ParamNoCheckout))
			_op.Checkout = false;
	}

	public override void Invoke()
	{
		Repository.Clone(_url, _path, _op);
	}
}
