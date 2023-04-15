using FarNet;
using LibGit2Sharp;
using System;

namespace GitKit;

abstract class BaseExplorer : Explorer
{
	readonly MyRepository _myRepository;

	public MyRepository MyRepository => _myRepository;
	public Repository Repository => _myRepository.Repository;

	public BaseExplorer(MyRepository repository, Guid typeId) : base(typeId)
	{
		_myRepository = repository;
	}
}
