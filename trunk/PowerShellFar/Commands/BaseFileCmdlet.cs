/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System.Collections.Generic;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of cmdlets working with panel files.
	/// </summary>
	/// <seealso cref="FarFile"/>
	/// <seealso cref="IPanel"/>
	public class BaseFileCmdlet : BaseCmdlet
	{
		/// <summary>
		/// See <see cref="IPanel.ShownFiles"/>.
		/// </summary>
		[Parameter(HelpMessage = "Get all the panel items.")]
		public SwitchParameter All
		{
			get { return _All; }
			set { _All = value; }
		}
		SwitchParameter _All;

		/// <summary>
		/// See <see cref="IPanel.SelectedFiles"/>.
		/// </summary>
		[Parameter(HelpMessage = "Get the selected panel items or the current one if none is selected.")]
		public SwitchParameter Selected
		{
			get { return _Selected; }
			set { _Selected = value; }
		}
		SwitchParameter _Selected;

		/// <summary>
		/// See <see cref="IFar.Panel2"/>.
		/// </summary>
		[Parameter(HelpMessage = "Get items from the passive panel. Default is from the active panel.")]
		public SwitchParameter Passive
		{
			get { return _Passive; }
			set { _Passive = value; }
		}
		SwitchParameter _Passive;

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
						if (!_realNames || !My.PathEx.IsPath(f.Name))
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
				return panel2.Path;

			string name = f.Name;
			if (name == "..")
				return panel2.Path;

			if (panel1.RealNames && My.PathEx.IsPath(name))
			{
				if (panel1 == panel2)
					return name;

				return My.PathEx.Combine(panel2.Path, My.PathEx.GetFileName(name));
			}

			return My.PathEx.Combine(panel2.Path, name);
		}

	}
}
