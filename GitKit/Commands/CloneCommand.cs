﻿using FarNet;
using GitKit.Extras;
using LibGit2Sharp;

namespace GitKit.Commands;

sealed class CloneCommand : AnyCommand
{
	readonly string _url;
	readonly string _path;
	readonly CloneOptions _op;

	public CloneCommand(CommandParameters parameters)
	{
		_url = parameters.GetRequiredString(Parameter.Url);

		_path = Host.GetFullPath(parameters.GetString(Parameter.Path, true));

		_op = new CloneOptions
		{
			IsBare = parameters.GetBool(Parameter.IsBare),
			RecurseSubmodules = parameters.GetBool(Parameter.RecurseSubmodules),
			FetchOptions =
			{
				Depth = parameters.GetValue<int>(Parameter.Depth)
			}
		};

		_op.FetchOptions.CredentialsProvider = Host.GetCredentialsHandler();

		if (parameters.GetBool(Parameter.NoCheckout))
			_op.Checkout = false;
	}

	public override void Invoke()
	{
		Repository.Clone(_url, _path, _op);
	}
}
