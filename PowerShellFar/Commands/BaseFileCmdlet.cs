using FarNet;
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

	internal class PathEnumerator(IEnumerable<FarFile> files, string path, bool realNames, bool joinRealNames) : MyEnumerator<string, FarFile>(files)
	{
		readonly string _path = path;
		readonly bool _realNames = realNames;
		readonly bool _joinRealNames = realNames && joinRealNames;

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

	internal static string GetCurrentPath(IPanel panel1, IPanel? panel2 = null)
	{
		panel2 ??= panel1;

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
