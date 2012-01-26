
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Stepper states. See <see cref="Stepper.State"/>, <see cref="Stepper.StateChanged"/>.
	/// </summary>
	public enum StepperState
	{
		/// <summary>
		/// Stepper is just created and processing is not started.
		/// </summary>
		None,
		/// <summary>
		/// Stepper is getting steps by invoking a step unit.
		/// </summary>
		Loading,
		/// <summary>
		/// Step sequence is being parsed for sanity checks.
		/// </summary>
		Parsing,
		/// <summary>
		/// Step processing is in progress.
		/// </summary>
		Stepping,
		/// <summary>
		/// All steps have been processed.
		/// </summary>
		Completed,
		/// <summary>
		/// An error has stopped the processing. See <see cref="Stepper.Error"/>.
		/// </summary>
		Failed
	}

	/// <summary>
	/// Invoker of steps (macros and script blocks) and units (scripts getting steps).
	/// </summary>
	/// <remarks>
	/// This class invokes sequences of asynchronous steps: macros and script
	/// blocks. The core gets control after each step, completes pending jobs
	/// and invokes the next step. This scenario is used to perform tricky
	/// operations impossible with synchronous code flow. In particular, it is
	/// very effective for testing: some steps perform UI actions without much
	/// programming, following script block steps check expected results.
	/// <para>
	/// It is possible to add steps during stepping. The current stepper object
	/// is exposed as <see cref="Actor.Stepper"/> to the current step. The step
	/// may call <c>$Psf.Stepper.Go()</c> to insert extra steps to be invoked
	/// immediately after the current.
	/// </para>
	/// <para>
	/// If a script block step returns another script block, that is it looks like <c>{{...}}</c>,
	/// then <see cref="IFar.PostStep2"/> is called for the next step instead of the usual
	/// <see cref="IFar.PostStep"/>. The returned script block normally starts modal UI
	/// (dialog, editor, and etc.). The next step in the sequence is invoked when modal
	/// UI has started.
	/// </para>
	/// <para>
	/// Any other output of script block steps is not allowed. Use <c>Write-Host</c>
	/// or <c>$Far.Write()</c> in order to write some text to the screen. Do not use
	/// other <c>Write-*</c> cmdlets, they may not work.
	/// </para>
	/// <para>
	/// Consider to use the cmdlet <c>Invoke-FarStepper</c> in order to invoke stepper scripts.
	/// Direct use of this class is needed in more complex scenarous.
	/// </para>
	/// </remarks>
	/// <example>Test-Stepper-.ps1, Test-Stepper+.ps1, Test-Dialog+.ps1.</example>
	public sealed class Stepper
	{
		const string DataVariableName = "Data";
		/// <summary>
		/// The only allowed running instance.
		/// </summary>
		internal static Stepper RunningInstance { get { return Stepper._RunningInstance; } }
		static Stepper _RunningInstance;
		/// <summary>
		/// Gets the current unit data (it is exposed as $Data for steps).
		/// </summary>
		/// <remarks>
		/// Local variables of one step are not available for another.
		/// Only global variables can be used for sharing data between steps.
		/// This hashtable and its variable $Data are designed for use in steps.
		/// As a result, the global scope is not polluted too match by variables.
		/// $Data is reset for each unit and removed when all units are processed.
		/// </remarks>
		public Hashtable Data { get { return _Data; } }
		readonly Hashtable _Data = new Hashtable();
		// Units
		readonly List<string> _units = new List<string>();
		ArrayList _steps = new ArrayList();
		/// <summary>
		/// New stepper.
		/// </summary>
		public Stepper()
		{ }
		/// <summary>
		/// Gets the total count of steps processed so far.
		/// </summary>
		public int StepCount { get { return _StepCount; } }
		int _StepCount;
		/// <summary>
		/// Gets the total count of step units processed so far.
		/// </summary>
		public int UnitCount { get { return _UnitIndex + 1; } }
		/// <summary>
		/// Current step index.
		/// </summary>
		int _StepIndex;
		/// <summary>
		/// Current unit index.
		/// </summary>
		int _UnitIndex = -1;
		/// <summary>
		/// Tells to ask a user to choose an action before each step.
		/// </summary>
		/// <remarks>
		/// This mode is used for troubleshooting, demonstrations, and etc.
		/// </remarks>
		public bool Ask { get; set; }
		/// <summary>
		/// Adds steps and starts processing or inserts steps after the current running step.
		/// </summary>
		/// <param name="steps">Macros and script blocks.</param>
		/// <remarks>
		/// If it is called in order to start stepping then normally it should be the last script command.
		/// </remarks>
		public void Go(object[] steps)
		{
			AssumeCanStep();

			// it is exceptional but possible, just do nothing
			if (steps == null)
				return;

			// just starting
			if (_steps.Count == 0)
			{
				_StepIndex = 0;
				InsertRange(0, steps);
				Far.Net.PostStep(Action);
			}
			// in progress
			else
			{
				InsertRange(_StepIndex + 1, steps);
			}
		}
		// Inserts steps at
		void InsertRange(int index, IEnumerable steps)
		{
			// state
			State = StepperState.Parsing;

			// steps
			foreach (object step in steps)
			{
				object insert = null;
				ScriptBlock block = Cast<ScriptBlock>.From(step);
				if (block != null)
				{
					insert = block;
				}
				else
				{
					string macro = Cast<string>.From(step);
					if (macro != null)
						insert = macro;
					else
						Throw("Invalid step: " + TypeName(step));
				}
				_steps.Insert(index, insert);
				++index;
			}
		}
		/// <summary>
		/// Adds a step unit script file.
		/// </summary>
		/// <param name="path">
		/// A script that gets macros and script blocks.
		/// Use either full paths or just names of scripts in the system path.
		/// Use of relative paths is not recommended with more than one unit.
		/// </param>
		/// <remarks>
		/// The unit file is added to the end of the internal unit queue.
		/// Later it is invoked in order to get macros and script blocks.
		/// Then they are invoked one by one.
		/// </remarks>
		public void AddFile(string path)
		{
			if (path == null) throw new ArgumentNullException("path");

			_units.Add(path);
		}
		/// <summary>
		/// Starts processing of added steps step units.
		/// </summary>
		/// <remarks>
		/// Normally this should be the last command in the script.
		/// If stepping is already in progress then this call is ignored.
		/// </remarks>
		public void Go()
		{
			AssumeCanStep();

			if (_RunningInstance == null)
				Far.Net.PostStep(Action);
		}
		/// <summary>
		/// Gets the current running step unit or null if there is none.
		/// </summary>
		public string CurrentUnit
		{
			get
			{
				if (_UnitIndex >= 0 && _UnitIndex < _units.Count)
					return _units[_UnitIndex];
				else
					return null;
			}
		}
		void AssumeCanStep()
		{
			if (_RunningInstance != this && _RunningInstance != null)
				throw new InvalidOperationException("Stepper is running, nested steppers are not allowed.");
		}
		/// <summary>
		/// Event triggered when the stepper state has changed.
		/// </summary>
		/// <remarks>
		/// It is mostly designed for handler which perform logging and diagnostics.
		/// <para>
		/// New state is exposed for a script handler as <c>$this.State</c>
		/// which is the <see cref="State"/> property of the current stepper.
		/// </para>
		/// </remarks>
		public event EventHandler StateChanged;
		StepperState _state_ = StepperState.None;
		/// <summary>
		/// Gets the current stepping state.
		/// </summary>
		/// <remarks>
		/// The state may be used for example by a <see cref="StateChanged"/> event handler.
		/// </remarks>
		public StepperState State
		{
			get { return _state_; }
			private set
			{
				// the same?
				if (value == _state_)
					return;

				// set the helper variable
				switch (value)
				{
					case StepperState.Loading:
					case StepperState.Stepping:
						A.Psf.Engine.SessionState.PSVariable.Set(DataVariableName, _Data);
						break;
				}

				// change
				_state_ = value;

				// trigger
				if (StateChanged != null)
					StateChanged(this, null);

				// remove the helper variable
				switch (value)
				{
					case StepperState.Completed:
					case StepperState.Failed:
						A.Psf.Engine.SessionState.PSVariable.Remove(DataVariableName);
						break;
				}
			}
		}
		Exception _Error;
		/// <summary>
		/// Gets the exception which has stopped stepping.
		/// </summary>
		public Exception Error
		{
			get { return _Error; }
		}
		// Invokes the next step in the step sequence.
		void Action()
		{
			AssumeCanStep();

			try
			{
				// this is running
				_RunningInstance = this;

				// done with steps?
				if (_StepIndex >= _steps.Count)
				{
					// remove user data
					if (_Data != null)
						_Data.Clear();

					// done with units?
					if (++_UnitIndex >= _units.Count)
					{
						// state
						State = StepperState.Completed;
						return;
					}

					// state
					State = StepperState.Loading;

					// reset steps
					_steps.Clear();
					_StepIndex = 0;

					// get steps from the current unit
					var path = _units[_UnitIndex];
					var code = "& '" + path.Replace("'", "''") + "'";
					Collection<PSObject> steps = A.InvokeCode(code);

					// no steps? 'continue'
					if (steps.Count == 0)
					{
						Far.Net.PostStep(Action);
						return;
					}

					// add steps and start
					InsertRange(0, steps);
				}

				// state
				State = StepperState.Stepping;

				// counter
				++_StepCount;

				// next step object
				object it = _steps[_StepIndex];
				if (it == null)
					Throw("Step is null.");

				// show
				if (Ask)
				{
					string text = it.ToString();
					string title = "Step " + (_StepIndex + 1) + "/" + _steps.Count;
					if (_units.Count > 0)
					{
						if (_UnitIndex >= 0)
							text = _units[_UnitIndex].ToString().Trim() + "\r" + text;
						title += " Unit " + (_UnitIndex + 1) + "/" + _units.Count;
					}

					switch (Far.Net.Message(
						text,
						title,
						MessageOptions.LeftAligned,
						new string[] { "Step", "Continue", "Cancel" }))
					{
						case 0:
							break;
						case 1:
							Ask = false;
							break;
						default:
							return;
					}
				}

				// invoke the next step
				ScriptBlock block = Cast<ScriptBlock>.From(it);
				if (block != null)
				{
					// invoke the step script
					Collection<PSObject> result = null;
					try
					{
						result = block.Invoke();
					}
					catch (RuntimeException ex)
					{
						Throw(block, "Step failed: " + ex.Message, ex);
					}

					// extra script, normally starts modal UI
					ScriptBlock script;
					if (result.Count == 1 && null != (script = result[0].BaseObject as ScriptBlock))
					{
						++_StepIndex;
						Far.Net.PostStep2(delegate { script.Invoke(); }, Action);
						return;
					}

					// unexpected output
					if (result.Count != 0)
					{
						Throw(block, string.Format(null,
							"Unexpected step output: {0} item(s): [{1}]...",
							result.Count, TypeName(result[0])), null);
					}
				}
				else
				{
					// post macro
					Far.Net.PostMacro(it.ToString());
				}

				// post
				++_StepIndex;
				Far.Net.PostStep(Action);
			}
			catch (Exception error)
			{
				_Error = error;
				State = StepperState.Failed;
				throw;
			}
			finally
			{
				// step is over
				_RunningInstance = null;
			}
		}
		static void Throw(ScriptBlock script, string message, Exception innerException)
		{
			throw new ModuleException(string.Format(null,
				"{0}\r\nFile: {1}\r\nLine: {2}\r\nCode: {{{3}}}",
				message,
				script.File,
				script.StartPosition.StartLine,
				script), innerException);
		}
		void Throw(string message)
		{
			throw new ModuleException(string.Format(null,
				"{0}\r\nUnit: {1}",
				message,
				CurrentUnit));
		}
		static string TypeName(object value)
		{
			var it = Cast<object>.From(value);
			return it == null ? "null" : it.GetType().FullName;
		}
	}
}
