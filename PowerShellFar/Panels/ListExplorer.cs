using FarNet;
using System;

namespace PowerShellFar;

/// <summary>
/// Base list explorer.
/// </summary>
/// <param name="typeId">.</param>
public abstract class ListExplorer(Guid typeId) : Explorer(typeId)
{
	internal bool SkipDeleteFiles { get; set; }
}
