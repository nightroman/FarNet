using FarNet;
using System;

namespace GitKit.Panels;

abstract class BaseExplorer(string gitDir, Guid typeId) : Explorer(typeId)
{
	public string GitDir => gitDir;
}
