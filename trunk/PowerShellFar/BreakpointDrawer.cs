
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Linq;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShell breakpoint drawer.
	/// </summary>
	[ModuleDrawer(Name = "PowerShell breakpoints", Mask = "*.ps1;*.psm1", Priority = 2)]
	[System.Runtime.InteropServices.Guid("67db13c5-6b7b-4936-b984-e59db08e23c7")]
	public class BreakpointDrawer : ModuleDrawer
	{
		/// <inheritdoc/>
		public override void Invoke(object sender, ModuleDrawerEventArgs e)
		{
			if (e == null) return;

			var editor = (IEditor)sender;
			var script = editor.FileName;
			var breakpoints = A.Psf.Breakpoints.Where(x => script.Equals(x.Script, StringComparison.OrdinalIgnoreCase));

			foreach (var line in e.Lines)
			{
				foreach (var bp in breakpoints)
				{
					if (bp.Line != line.Index + 1)
						continue;

					e.Colors.Add(new EditorColor(
						line.Index,
						0,
						e.EndChar,
						ConsoleColor.White,
						ConsoleColor.DarkRed));

					break;
				}
			}
		}
	}
}
