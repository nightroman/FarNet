using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class CloneCommand : AnyCommand
{
	readonly string _url;
	readonly string _path;
	readonly CloneOptions _op;

	public CloneCommand(DbConnectionStringBuilder parameters)
	{
		_url = parameters.GetRequired(Parameter.Url);

		_path = Host.GetFullPath(parameters.GetValue(Parameter.Path));

		_op = new CloneOptions
		{
			IsBare = parameters.GetValue<bool>(Parameter.IsBare),
			RecurseSubmodules = parameters.GetValue<bool>(Parameter.RecurseSubmodules),
			CredentialsProvider = Host.GetCredentialsHandler()
		};

		if (parameters.GetValue<bool>(Parameter.NoCheckout))
			_op.Checkout = false;
	}

	public override void Invoke()
	{
		Repository.Clone(_url, _path, _op);
	}
}
