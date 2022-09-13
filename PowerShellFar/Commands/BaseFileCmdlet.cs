
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFar.Commands;

class BaseFileCmdlet : BaseCmdlet
{
	[Parameter(ParameterSetName = "All", Mandatory = true)]
	public SwitchParameter All { get; set; }

	[Parameter(ParameterSetName = "Selected", Mandatory = true)]
	public SwitchParameter Selected { get; set; }

	[Parameter]
	public SwitchParameter Passive { get; set; }

	internal class PathEnumerator : My.Enumerator<string, FarFile>
	{
		readonly string _path;
		readonly bool _realNames;
		readonly bool _joinRealNames;

		public PathEnumerator(IEnumerable<FarFile> files, string path, bool realNames, bool joinRealNames) : base(files)
		{
			_path = path;
			_realNames = realNames;
			_joinRealNames = realNames && joinRealNames;
		}

		public override bool MoveNext()
		{
			while (_enumerator.MoveNext())
			{
				var file = _enumerator.Current;
				if (file.Name != "..")
				{
					if (!_realNames || !My.PathEx.IsFSPath(file.Name))
						_current = My.PathEx.Combine(_path, file.Name);
					else if (_joinRealNames)
						_current = My.PathEx.Combine(_path, My.PathEx.GetFileName(file.Name));
					else
						_current = file.Name;
					return true;
				}
			}
			return false;
		}
	}

	internal static string GetCurrentPath(IPanel panel1, IPanel panel2)
	{
		var file = panel1.CurrentFile;
		if (file is null)
			return panel2.CurrentDirectory;

		var name = file.Name;
		if (name == "..")
			return panel2.CurrentDirectory;

		if (panel1.RealNames && My.PathEx.IsFSPath(name))
		{
			if (panel1 == panel2)
				return name;

			return My.PathEx.Combine(panel2.CurrentDirectory, My.PathEx.GetFileName(name));
		}

		return My.PathEx.Combine(panel2.CurrentDirectory, name);
	}
}
