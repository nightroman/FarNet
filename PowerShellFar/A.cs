/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using FarNet;

namespace PowerShellFar
{
	static class A
	{
		/// <summary>PowerShellFar actor.</summary>
		public static Actor Psf { get { return _Psf_; } }
		static Actor _Psf_;

		/// <summary>FarNet factory.</summary>
		public static IFar Far { get { return _Far_; } }
		static IFar _Far_;

		public static void Connect(Actor psf, IFar far)
		{
			_Psf_ = psf;
			_Far_ = far;
		}

		/// <summary>
		/// Shows an error.
		/// </summary>
		public static void Msg(Exception error)
		{
			Far.Msg(error.Message, "PowerShellFar error", MsgOptions.LeftAligned);
		}

		/// <summary>
		/// Shows a message.
		/// </summary>
		public static void Msg(string message)
		{
			Far.Msg(message, Res.Name, MsgOptions.LeftAligned);
		}

		/// <summary>
		/// Creates standard Far editor ready for opening (F4)
		/// </summary>
		/// <param name="filePath">Existing file to edit.</param>
		public static IEditor CreateEditor(string filePath)
		{
			IEditor edit = Far.CreateEditor();
			edit.FileName = filePath;
			return edit;
		}

		/// <summary>
		/// Creates standard Far viewer ready for opening (F3)
		/// </summary>
		/// <param name="filePath">Existing file to view.</param>
		public static IViewer CreateViewer(string filePath)
		{
			IViewer view = Far.CreateViewer();
			view.FileName = filePath;
			return view;
		}

		/// <summary>
		/// Sets an object member value.
		/// </summary>
		public static void SetMemberValue(PSPropertyInfo info, object value)
		{
			//! SetValueInvocationException (RuntimeException is its ancestor)
			info.Value = value;
		}

		/// <summary>
		/// Sets an item property value as it is.
		/// </summary>
		public static void SetPropertyValue(string itemPath, string propertyName, object value)
		{
			//! setting PSPropertyInfo.Value is not working, so do use Set-ItemProperty
			Psf.InvokeCode(
				"Set-ItemProperty -LiteralPath $args[0] -Name $args[1] -Value $args[2] -ErrorAction Stop",
				itemPath, propertyName, value);
		}

		// Sets location (with workaround)
		public static void SetLocation(string path)
		{
			Psf.Engine.SessionState.Path.SetLocation(Kit.EscapeWildcard(path));
		}

		/// <summary>
		/// Writes invocation errors.
		/// </summary>
		public static void WriteErrors(TextWriter writer, IEnumerable errors)
		{
			if (errors == null)
				return;

			foreach (object error in errors)
			{
				writer.WriteLine("ERROR:");
				writer.WriteLine(error.ToString());

				ErrorRecord asErrorRecord = Convert<ErrorRecord>.From(error);
				if (asErrorRecord != null && asErrorRecord.InvocationInfo != null)
					writer.WriteLine(Kit.PositionMessage(asErrorRecord.InvocationInfo.PositionMessage));
			}
		}

		/// <summary>
		/// Writes invocation exception.
		/// </summary>
		public static void WriteException(TextWriter writer, Exception ex)
		{
			if (ex == null)
				return;

			writer.WriteLine("ERROR:");
			writer.WriteLine(ex.GetType().Name + ":");
			writer.WriteLine(ex.Message);

			RuntimeException asRuntimeException = ex as RuntimeException;
			if (asRuntimeException != null && asRuntimeException.ErrorRecord != null && asRuntimeException.ErrorRecord.InvocationInfo != null)
				writer.WriteLine(Kit.PositionMessage(asRuntimeException.ErrorRecord.InvocationInfo.PositionMessage));
		}

		/// <summary>
		/// Sets an item content, shows errors.
		/// </summary>
		public static bool SetContentUI(string itemPath, string text)
		{
			//! PSF only, can do in console:
			//! as a script: if param name -Value is used, can't set e.g. Alias:\%
			//! as a block: same problem
			//! so, use a command
			using (PowerShell p = Psf.CreatePipeline())
			{
				Command command = new Command("Set-Content");
				command.Parameters.Add("LiteralPath", itemPath);
				command.Parameters.Add("Value", text);
				command.Parameters.Add(Prm.Force);
				command.Parameters.Add(Prm.EAContinue);
				p.Commands.AddCommand(command);
				p.Invoke();
				if (ShowError(p))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Outputs to the default formatter.
		/// </summary>
		/// <param name="ps">Pipeline.</param>
		/// <param name="input">Input objects.</param>
		public static void Out(PowerShell ps, IEnumerable input)
		{
			ps.Commands.AddCommand("Out-Default");
			ps.Invoke(input);
		}

		/// <summary>
		/// Outputs an exception or its error record.
		/// </summary>
		/// <param name="ps">Pipeline.</param>
		/// <param name="ex">Exception.</param>
		public static void OutReason(PowerShell ps, Exception ex)
		{
			RuntimeException asRuntimeException = ex as RuntimeException;
			object error;
			if (asRuntimeException == null)
				error = ex;
			else
				error = asRuntimeException.ErrorRecord;
			
			Out(ps, new object[] { "ERROR\n" + ex.GetType().Name + ":", error });
		}

		/// <summary>
		/// Shows errors, if any, in a message box and returns true, else just returns false.
		/// </summary>
		public static bool ShowError(PowerShell ps)
		{
			if (ps.Streams.Error.Count == 0)
				return false;

			StringBuilder sb = new StringBuilder();
			foreach (object o in ps.Streams.Error)
				sb.AppendLine(o.ToString());
			
			Far.Msg(sb.ToString(), "PowerShellFar error(s)", MsgOptions.LeftAligned);
			return true;
		}

		/// <summary>
		/// Sets current directory, shows errors but does not throw.
		/// </summary>
		/// <param name="currentDirectory">null or a path.</param>
		public static void SetCurrentDirectoryFinally(string currentDirectory)
		{
			if (currentDirectory != null)
			{
				try
				{
					Directory.SetCurrentDirectory(currentDirectory);
				}
				catch (Exception ex)
				{
					Far.ShowError(Res.Name, ex);
				}
			}
		}

	}
}
