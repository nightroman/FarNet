﻿using LibGit2Sharp;
using System.Data.Common;

namespace GitKit;

sealed class CloneCommand : AnyCommand
{
	readonly string _url;
	readonly string _path;
	readonly CloneOptions _op;

	public CloneCommand(string value, DbConnectionStringBuilder parameters)
	{
		_url = value;

		_path = Host.GetFullPath(parameters.GetValue("Path"));

		_op = new CloneOptions
		{
			IsBare = parameters.GetValue<bool>("IsBare"),
			RecurseSubmodules = parameters.GetValue<bool>("RecurseSubmodules"),
			CredentialsProvider = Lib.GitCredentialsHandler
		};

		if (parameters.GetValue<bool>("NoCheckout"))
			_op.Checkout = false;
	}

	public override void Invoke()
	{
		Repository.Clone(_url, _path, _op);
	}
}