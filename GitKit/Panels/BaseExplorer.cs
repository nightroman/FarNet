using FarNet;
using LibGit2Sharp;
using System;

namespace GitKit.Panels;

abstract class BaseExplorer(Repository repository, Guid typeId) : Explorer(typeId)
{
	public Repository Repository { get; } = repository;
}
