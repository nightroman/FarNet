
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;
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
			var fullPath = Path.GetFullPath(editor.FileName); //!
			var breakpoints = A.Psf.Breakpoints.Where(x => fullPath.Equals(x.Script, StringComparison.OrdinalIgnoreCase));

			bool? isColorer = null;
			var guidColorer = new Guid("d2f36b62-a470-418d-83a3-ed7a3710e5b5");

			foreach (var line in e.Lines)
			{
				foreach (var bp in breakpoints)
				{
					if (bp.Line != line.Index + 1)
						continue;

					var colors = editor.GetColors(line.Index);
					if (!isColorer.HasValue && colors.Count > 0)
						isColorer = colors.FirstOrDefault(x => x.Owner == guidColorer) != null;

					if (isColorer.GetValueOrDefault())
					{
						// foreground: keep original but replace yellow and white with black
						// background: yellow
						foreach (var color in colors)
						{
							e.Colors.Add(new EditorColor(
							line.Index,
							color.Start,
							color.End,
							(color.Foreground == ConsoleColor.Yellow || color.Foreground == ConsoleColor.White) ? ConsoleColor.Black : color.Foreground,
							ConsoleColor.Yellow));
						}
					}
					else
					{
						// color all black on yellow
						e.Colors.Add(new EditorColor(
							line.Index,
							e.StartChar,
							e.EndChar,
							ConsoleColor.Black,
							ConsoleColor.Yellow));
					}

					// with 2+ bp per line coloring one is enough, we color whole line for simplicity
					break;
				}
			}
		}
	}
}
