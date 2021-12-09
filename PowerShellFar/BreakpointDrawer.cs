
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.IO;
using System.Linq;

namespace PowerShellFar
{
	/// <summary>
	/// PowerShell breakpoint drawer.
	/// </summary>
	[ModuleDrawer(Name = "PowerShell breakpoints", Mask = "*.ps1;*.psm1", Priority = 1)]
	[System.Runtime.InteropServices.Guid("67db13c5-6b7b-4936-b984-e59db08e23c7")]
	public class BreakpointDrawer : ModuleDrawer
	{
		/// <inheritdoc/>
		public override void Invoke(IEditor editor, ModuleDrawerEventArgs e)
		{
			if (editor == null || e == null) return;

			var fullPath = Path.GetFullPath(editor.FileName); //!
			var breakpoints = A.Psf.Breakpoints.Where(x => fullPath.Equals(x.Script, StringComparison.OrdinalIgnoreCase));

			foreach (var line in e.Lines)
			{
				foreach (var bp in breakpoints)
				{
					if (bp.Line != line.Index + 1)
						continue;

					var colors = editor.GetColors(line.Index);
					foreach (var color in colors)
					{
						// foreground: keep original but replace yellow and white with black (assume FarColorer)
						// background: yellow
						e.Colors.Add(new EditorColor(
							line.Index,
							color.Start,
							color.End,
							(color.Foreground == ConsoleColor.Yellow || color.Foreground == ConsoleColor.White) ? ConsoleColor.Black : color.Foreground,
							ConsoleColor.Yellow));
					}
					break;
				}
			}
		}
	}
}
