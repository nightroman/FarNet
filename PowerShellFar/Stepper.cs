
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Threading.Tasks;

namespace PowerShellFar
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	sealed class Stepper : IEnumerator<object>, IEnumerable<object>
	{
		public class Args
		{
			public Args(string filePath)
			{
				FileName = filePath ?? throw new ArgumentNullException("path");
			}
			public string FileName { get; private set; }
			public bool Confirm { get; set; }
		}

		const string DataVariableName = "Data";
		readonly Args _args;
		object _current;
		object[] _steps;
		int _index = -1;
		TaskCompletionSource<object> _tcs;
		readonly Hashtable _data = new Hashtable(StringComparer.OrdinalIgnoreCase);

		public object Current { get { return _current; } }
		public void Reset() { throw new NotImplementedException(); }
		public IEnumerator<object> GetEnumerator() { return this; }
		IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this; }

		static public void Run(Args args)
		{
			var stepper = new Stepper(args);
			Far.Api.PostSteps(stepper);
		}

		static public Task RunAsync(Args args)
		{
			var stepper = new Stepper(args)
			{
				_tcs = new TaskCompletionSource<object>()
			};
			Far.Api.PostSteps(stepper);
			return stepper._tcs.Task;
		}

		Stepper(Args args)
		{
			_args = args;
		}

		void Init()
		{
			A.Psf.Engine.SessionState.PSVariable.Set(DataVariableName, _data);
			using (var ps = A.Psf.NewPowerShell())
			{
				var steps = ps.AddCommand(_args.FileName).Invoke();
				_steps = new object[steps.Count];
				for (int i = 0; i < steps.Count; ++i)
				{
					object step = steps[i];
					ScriptBlock block = Cast<ScriptBlock>.From(step);
					if (block != null)
					{
						step = block;
					}
					else
					{
						string macro = Cast<string>.From(step);
						if (macro != null)
							step = macro;
						else
							throw NewError("Invalid step: " + TypeName(step));
					}
					_steps[i] = step;
				}
			}
		}

		public void Dispose()
		{
			if (_steps == null)
				return;

			A.Psf.Engine.SessionState.PSVariable.Remove(DataVariableName);
			_steps = null;
			_data.Clear();
		}

		public bool MoveNext()
		{
			_current = null;
			try
			{
				// init?
				if (_index < 0)
					Init();

				// dead?
				if (_steps == null)
					return false;

				// next index, done?
				if (++_index >= _steps.Length)
				{
					Dispose();
					_tcs?.SetResult(null);
					return false;
				}

				// currect step object
				var step = _steps[_index];

				// show prompt
				if (_args.Confirm)
				{
					var text = step.ToString();
					var title = $"Step {_index + 1}/{_steps.Length}";
					var args = new MessageArgs()
					{
						Text = text,
						Caption = title,
						Options = MessageOptions.LeftAligned,
						Buttons = new string[] { "Step", "Continue", "Cancel" },
						Position = new Point(int.MaxValue, 1)
					};
					switch (Far.Api.Message(args))
					{
						case 0:
							break;
						case 1:
							_args.Confirm = false;
							break;
						default:
							Dispose();
							_tcs?.SetResult(null);
							return false;
					}
				}

				// invoke currect step
				if (step is ScriptBlock block)
				{
					Collection<PSObject> result = null;
					try
					{
						result = block.Invoke();
					}
					catch (RuntimeException exn)
					{
						throw NewError(block, $"Step error: {exn.Message}\r\n{exn.ErrorRecord.InvocationInfo.PositionMessage}", exn);
					}

					// modal script, tell the core to post it as enumerator `Current` object
					if (result.Count == 1 && result[0].BaseObject is ScriptBlock script)
					{
						_current = new Action(() =>
						{
							try
							{
								script.Invoke();
							}
							catch (RuntimeException exn)
							{
								Dispose();
								var error = NewError(script, $"Step error: {exn.Message}\r\n{exn.ErrorRecord.InvocationInfo.PositionMessage}", exn);
								if (_tcs == null)
									throw error;
								else
									_tcs.SetException(error);
							}
						});
						return true;
					}

					// unexpected output
					if (result.Count != 0)
						throw NewError(block, $"Unexpected step output: {result.Count} item(s): [{TypeName(result[0])}]...", null);
				}
				else
				{
					// post macro
					_current = step;
				}

				// post
				return true;
			}
			catch (Exception exn)
			{
				Dispose();

				if (_tcs == null)
					throw;
				
				_tcs.SetException(exn);
				return false;
			}
		}

		static Exception NewError(ScriptBlock script, string message, Exception innerException)
		{
			return new ErrorException(string.Format(null,
				"{0}\r\nAt {1}:{2}\r\nCode: {{{3}}}",
				message,
				script.File,
				script.StartPosition.StartLine,
				script), innerException);
		}

		Exception NewError(string message)
		{
			return new ErrorException($"{message}\r\n{_args.FileName}");
		}

		static string TypeName(object value)
		{
			var it = Cast<object>.From(value);
			return it == null ? "null" : it.GetType().FullName;
		}
	}
}
