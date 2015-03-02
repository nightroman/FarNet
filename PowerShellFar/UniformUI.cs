
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// UI output modes.
	/// </summary>
	enum WriteMode
	{
		None,
		Debug,
		Error,
		Verbose,
		Warning
	};

	/// <summary>
	/// Base PowerShell host user interface.
	/// </summary>
	/// <remarks>
	/// Basically all writing methods are called in here and they recall a writer.
	/// Writers are easy to change dynamically for the same UI instance.
	/// For example push/pop logic is used in <see cref="FarUI"/>.
	/// </remarks>
	abstract class UniformUI : PSHostUserInterface
	{
		internal bool HasError { get; set; }

		internal abstract OutputWriter Writer { get; }

		protected virtual void Writing() { }

		static protected PSObject ValueToResult(string value, bool safe)
		{
			object r;
			if (safe)
			{
				var ss = new SecureString();
				r = ss;
				foreach (var c in value)
					ss.AppendChar(c);
			}
			else
			{
				r = value;
			}
			return new PSObject(r);
		}

		#region PSHostUserInterface
		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			throw new NotImplementedException();
		}
		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			throw new NotImplementedException();
		}
		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			throw new NotImplementedException();
		}
		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			return NativeMethods.PromptForCredential(caption, message, userName, targetName, allowedCredentialTypes, options);
		}
		public override PSHostRawUserInterface RawUI
		{
			get { return new RawUI(); }
		}
		public override string ReadLine()
		{
			throw new NotImplementedException();
		}
		public override SecureString ReadLineAsSecureString()
		{
			if (Far.Api.UI.IsCommandMode)
			{
				for (; ; )
				{
					var ui = new UI.ReadLine() { Password = true };
					if (!ui.Show())
					{
						A.AskStopPipeline();
						continue;
					}

					WriteLine("*");
					return (SecureString)ValueToResult(ui.Text, true).BaseObject;
				}
			}

			const string name = " ";
			var field = new FieldDescription(name);
			field.SetParameterType(typeof(SecureString));
			var fields = new Collection<FieldDescription>() { field };

			var r = Prompt("", "", fields);
			if (r == null)
				return null;

			return (SecureString)r[name].BaseObject;
		}
		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
		}
		public sealed override void Write(string value)
		{
			Writing();
			Writer.Write(value);
		}
		public sealed override void WriteLine()
		{
			Writing();
			Writer.WriteLine();
		}
		public sealed override void WriteLine(string value)
		{
			Writing();
			Writer.WriteLine(value);
		}
		public sealed override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Writing();
			Writer.Write(foregroundColor, backgroundColor, value);
		}
		public sealed override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			Writing();
			Writer.WriteLine(foregroundColor, backgroundColor, value);
		}
		public sealed override void WriteDebugLine(string message)
		{
			Writing();
			Writer.WriteDebugLine(message);
		}
		public sealed override void WriteErrorLine(string value)
		{
			HasError = true;
			Writing();
			Writer.WriteErrorLine(value);
		}
		public sealed override void WriteVerboseLine(string message)
		{
			Writing();
			Writer.WriteVerboseLine(message);
		}
		public sealed override void WriteWarningLine(string message)
		{
			Writing();
			Writer.WriteWarningLine(message);
		}
		#endregion
	}
}
