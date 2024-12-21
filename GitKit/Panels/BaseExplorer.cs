using FarNet;
using System;

namespace GitKit.Panels;

abstract class BaseExplorer(string gitRoot, Guid typeId) : Explorer(typeId)
{
	public string GitRoot => gitRoot;
}
