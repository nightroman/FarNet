/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Management.Automation;
using System.Text;
using FarNet;

namespace FarMacro
{
	[Cmdlet(VerbsData.Edit, BaseCmdlet.Noun)]
	[Description("Opens the editor with the macro sequence and install the macro on saving if its syntax is correct.")]
	public class EditFarMacroCommand : BaseCmdlet
	{
		[Parameter(
			ParameterSetName = "Name",
			Position = 0,
			Mandatory = true,
			HelpMessage = "The key name. It is corrected to the standard form.")]
		public string Name { get; set; }

		[Parameter(
			ParameterSetName = "Name",
			Position = 1,
			HelpMessage = "The macro area. If it is not set then the current Dialog, Editor, Shell, or Viewer area is assumed.")]
		public MacroArea Area { get; set; }

		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Input macro instances.", ParameterSetName = "Macros")]
		public Macro InputObject { get; set; }

		[Parameter(
			ParameterSetName = "Macro",
			HelpMessage = "The macro instance which sequence is edited.")]
		public Macro Macro { get; set; }

		[Parameter(
			ParameterSetName = "File",
			HelpMessage = "The path of a file with a macro sequence to edit.")]
		public string FilePath { get; set; }

		/// <summary>
		/// Actual file path used by the editor.
		/// </summary>
		string FileName;

		/// <summary>
		/// The only used editor.
		/// </summary>
		IEditor Editor;

		/// <summary>
		/// The last parser error or null.
		/// </summary>
		MacroParseError Error;

		static class m
		{
			public const string
				MacroFileExtension = ".macro",
				InvalidMacro = "The macro sequence is not valid.",
				ContinueChanges = "Continue changes",
				DiscardChanges = "Discard changes",
				ExitAnyway = "Exit anyway",
				InvalidKeyName = "Invalid key name.",
				UndefinedArea = "Parameter 'Area' has to be defined.";
		}

		protected override void BeginProcessing()
		{
			// get the working file path and the macro
			if (FilePath != null)
			{
				// file is provided
				FileName = FilePath;
			}
			else
			{
				// macro is provided, use it
				if (Macro != null)
				{
					Area = Macro.Area;
					Name = Macro.Name;
				}

				// validate the key name
				Name = Name.Replace("(Slash)", "/");
				int code = Far.Net.NameToKey(Name);
				if (code >= 0)
					Name = Far.Net.KeyToName(code);
				else
					throw new ArgumentException(m.InvalidKeyName);

				// validate the area
				if (Area == MacroArea.Root)
				{
					switch (Far.Net.Window.Kind)
					{
						case WindowKind.Dialog: Area = MacroArea.Dialog; break;
						case WindowKind.Editor: Area = MacroArea.Editor; break;
						case WindowKind.Panels: Area = MacroArea.Shell; break;
						case WindowKind.Viewer: Area = MacroArea.Viewer; break;
						default: throw new ArgumentException(m.UndefinedArea);
					}
				}

				// get or create the macro, if not yet
				if (Macro == null)
				{
					Macro = Far.Net.Macro.GetMacro(Area, Name);
					if (Macro == null)
					{
						Macro = new Macro();
						Macro.Area = Area;
						Macro.Name = Name;
					}
				}

				// make a valid, unique and reusable name with .macro extension for Colorer
				FileName = Macro.Name;
				foreach (char bad in Path.GetInvalidFileNameChars())
				{
					if (FileName.IndexOf(bad) >= 0)
						FileName = FileName.Replace(new string(new char[] { bad }), "_" + (int)bad + "_");
				}
				FileName = Path.Combine(MyAppData, Area.ToString() + "." + FileName + m.MacroFileExtension);

				// write the sequence to the file
				File.WriteAllText(FileName, Macro.Sequence, Encoding.Unicode);
			}

			// setup the editor
			//! use history even in Macro mode, names are reusable
			Editor = Far.Net.CreateEditor();
			Editor.FileName = FileName;

			// code page and the title for the macro
			if (FilePath == null)
			{
				Editor.CodePage = Encoding.Unicode.CodePage;
				Editor.Title = Area.ToString() + " " + Name;
			}

			// add events
			Editor.Saving += OnSaving;
			Editor.Closed += OnClosed;

			// save macros
			if (FilePath == null)
				Far.Net.Macro.Save();

			// go
			Editor.Open();
		}

		void OpenEditor()
		{
			// go to the last error position
			if (Error == null)
				Editor.Frame = new TextFrame(-1);
			else
				Editor.GoTo(Error.Pos, Error.Line);

			// open
			Editor.Open();
		}

		void OnSaving(object sender, EventArgs e)
		{
			// check the macro
			string sequence = Editor.GetText().TrimEnd();
			Error = Far.Net.Macro.Check(sequence, false);

			// go to the error position
			if (Error != null)
			{
				Editor.GoTo(Error.Pos, Error.Line);
			}
			// install the macro and load macros
			else if (FilePath == null)
			{
				//! update the macro, it can be external
				Macro.Sequence = sequence;
				Far.Net.Macro.Install(Macro);
				Far.Net.Macro.Load();
			}
		}

		void OnClosed(object sender, EventArgs e)
		{
			// case: OK
			if (Error == null)
			{
				// kill the temporary file
				if (FilePath == null)
					File.Delete(FileName);

				return;
			}
			
			// case: error in Macro mode
			if (FilePath == null)
			{
				if (1 != Far.Net.Message(m.InvalidMacro, Noun, 0, new string[] { m.ContinueChanges, m.DiscardChanges }))
					OpenEditor();
			}
			// case: error in File mode
			else
			{
				if (1 != Far.Net.Message(m.InvalidMacro, Noun, 0, new string[] { m.ContinueChanges, m.ExitAnyway }))
					OpenEditor();
			}
		}
	}
}
