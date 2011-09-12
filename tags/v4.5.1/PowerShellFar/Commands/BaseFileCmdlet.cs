
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	class BaseFileCmdlet : BaseCmdlet
	{
		[Parameter(ParameterSetName = "All", Mandatory = true)]
		public SwitchParameter All { get; set; }
		[Parameter(ParameterSetName = "Selected", Mandatory = true)]
		public SwitchParameter Selected { get; set; }
		[Parameter()]
		public SwitchParameter Passive { get; set; }
		internal class PathEnumerator : My.Enumerator<string, FarFile>
		{
			string _path;
			bool _realNames;
			bool _joinRealNames;
			public PathEnumerator(IEnumerable<FarFile> files, string path, bool realNames, bool joinRealNames)
				: base(files)
			{
				_path = path;
				_realNames = realNames;
				_joinRealNames = realNames && joinRealNames;
			}
			public override bool MoveNext()
			{
				while (_enumerator.MoveNext())
				{
					FarFile f = _enumerator.Current;
					if (f.Name != "..")
					{
						if (!_realNames || !My.PathEx.IsFSPath(f.Name))
							_current = My.PathEx.Combine(_path, f.Name);
						else if (_joinRealNames)
							_current = My.PathEx.Combine(_path, My.PathEx.GetFileName(f.Name));
						else
							_current = f.Name;
						return true;
					}
				}
				return false;
			}
		}
		internal static string GetCurrentPath(IPanel panel1, IPanel panel2)
		{
			FarFile f = panel1.CurrentFile;
			if (f == null)
				return panel2.CurrentDirectory;

			string name = f.Name;
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
}
