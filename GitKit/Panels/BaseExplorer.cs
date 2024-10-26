using FarNet;
using LibGit2Sharp;
using System;

namespace GitKit.Panels;

abstract class BaseExplorer : Explorer
{
	public Repository Repository { get; }

	public BaseExplorer(Repository repository, Guid typeId) : base(typeId)
	{
		Repository = repository;
	}
}
