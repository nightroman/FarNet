
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2011 Roman Kuzmin
*/

using System;
using System.IO;
using System.Management.Automation;
using System.Text;
using FarNet;

namespace FarMacro
{
	[Cmdlet(VerbsData.Edit, BaseCmdlet.Noun)]
	public class EditFarMacroCommand : BaseCmdlet
	{
		[Parameter(ParameterSetName = "Name", Position = 0, Mandatory = true)]
		public string Name { get; set; }
		[Parameter(ParameterSetName = "Name", Position = 1)]
		public MacroArea Area { get; set; }
		[Parameter(Position = 0, Mandatory = true, ValueFromPipeline = true, ParameterSetName = "Macros")]
		public Macro InputObject { get; set; }
		[Parameter(ParameterSetName = "Macro")]
		public Macro Macro { get; set; }
		[Parameter(ParameterSetName = "File")]
		public string FilePath { get; set; }
		[Parameter]
		public Panel Panel { get; set; }
		/// <summary>
		/// Actual file path used by the editor.
		/// </summary>
		string FileName;
		/// <summary>
		/// Original input scalar value.
		/// </summary>
		object InputScalar;
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
		/// <summary>
		/// Keeps the input value and converts it to text.
		/// </summary>
		string InputScalarToText(object value)
		{
			InputScalar = value ?? string.Empty;
			return InputScalar.ToString();
		}
		/// <summary>
		/// Converts to the original input type if needed.
		/// </summary>
		object TextToOutputScalar(string text)
		{
			if (InputScalar is string)
				return text;
			else
				return Kit.ConvertTo(text, InputScalar.GetType());
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
				if (Area != MacroArea.Consts && Area != MacroArea.Vars)
				{
					Name = Name.Replace("(Slash)", "/");
					int code = Far.Net.NameToKey(Name);
					if (code >= 0)
						Name = Far.Net.KeyToName(code);
					else
						throw new ArgumentException(m.InvalidKeyName);

					// validate the area
					if (Area == MacroArea.None)
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
				string extension = Area == MacroArea.Consts ? ".tmp" : m.MacroFileExtension;
				FileName = Path.Combine(TempPath(), Area.ToString() + "." + FileName + extension);

				// target text
				string text;
				if (Area == MacroArea.Consts)
					text = InputScalarToText(Far.Net.Macro.GetConstant(Name));
				else if (Area == MacroArea.Vars)
					text = InputScalarToText(Far.Net.Macro.GetVariable(Name));
				else
					text = Macro.Sequence;

				// write the text to the file
				File.WriteAllText(FileName, text, Encoding.Unicode);
			}

			// setup the editor
			//! allow history even in Macro mode, names are reusable
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
			// target text
			string text = Editor.GetText().TrimEnd();

			// case: Consts
			if (Area == MacroArea.Consts)
			{
				Far.Net.Macro.InstallConstant(Name, TextToOutputScalar(text));
			}
			// case: Vars
			else if (Area == MacroArea.Vars)
			{
				Far.Net.Macro.InstallVariable(Name, TextToOutputScalar(text));
			}
			else
			{
				// check the macro
				Error = Far.Net.Macro.Check(text, false);

				// go to the error position
				if (Error != null)
				{
					Editor.GoTo(Error.Pos, Error.Line);
					return;
				}
				// install the macro and load macros
				else if (FilePath == null)
				{
					//! update the macro, it can be external
					Macro.Sequence = text;
					Far.Net.Macro.Install(Macro);
				}
			}

			// update the panel
			if (Panel != null)
				Panel.Update(true);
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
