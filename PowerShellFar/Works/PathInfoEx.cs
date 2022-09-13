
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Management.Automation;

namespace PowerShellFar;

/// <summary>
/// Extra <see cref="PathInfo"/>.
/// </summary>
class PathInfoEx
{
	readonly PathInfo _PathInfo;
	string? _Path;

	///
	public PathInfoEx(string? path)
	{
		var core = A.Psf.Engine.SessionState.Path;
		if (string.IsNullOrEmpty(path) || path == ".")
			_PathInfo = core.CurrentLocation;
		else
			// 3 times faster than push/set/pop location; NB: it is slow anyway
			_PathInfo = core.GetResolvedPSPathFromPSPath(Kit.EscapeWildcard(path))[0];
	}

	internal PathInfoEx(PathInfo pathInfo)
	{
		_PathInfo = pathInfo;
	}

	/// <summary>
	/// Gets the friendly path.
	/// </summary>
	public string Path
	{
		get //_110318_140817
		{
			if (_Path == null)
			{
				_Path = _PathInfo.ProviderPath;
				if (!_Path.StartsWith("\\\\", StringComparison.Ordinal))
				{
					_Path = _PathInfo.Path;
					if ((_Path.Length == 0 || _Path == "\\") && _PathInfo.Drive != null)
						_Path = _PathInfo.Drive.Name + ":\\";
				}
			}
			return _Path;
		}
	}

	/// <summary>
	/// Gets the provider info.
	/// </summary>
	public ProviderInfo Provider => _PathInfo.Provider;

	/// <summary>
	/// Gets the drive name or null.
	/// </summary>
	//! 110227 PathInfo.Drive can be null even if a drive exists
	internal string? DriveName => _PathInfo.Drive?.Name;
}
