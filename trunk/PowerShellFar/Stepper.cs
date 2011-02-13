
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Text;
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
	/// Invoker of steps: key sequences, script blocks and scripts returning steps.
	/// </summary>
	/// <remarks>
	/// Requirement: .NET menu item must have a hotkey (set by [F4]) in the Far plugin menu [F11].
	/// <para>
	/// This object is exposed as <see cref="Actor.Stepper"/> to the step script block being invoked.
	/// A step may call <c>$Psf.Stepper.Go()</c> to insert extra steps to be invoked immediately after.
	/// </para>
	/// <para>
	/// Output of script blocks is normally ignored (except a script block or new keys, see below),
	/// use <c>Write-Host</c> or <c>$Far.Write()</c> to write some information to the screen.
	/// Do not call other <c>Write-*</c> cmdlets, they may fail because streams may be not opened.
	/// </para>
	/// <para>
	/// If a script block being invoked returns a string or another script block then <see cref="IFar.PostStepAfterKeys"/>
	/// or <see cref="IFar.PostStepAfterStep"/> is called for the next step instead of usual <see cref="IFar.PostStep"/>.
	/// The returned keys or script normally start modal UI (dialog, editor and etc.). The next step in the sequence
	/// will be invoked when modal UI element has started.
	/// </para>
	/// </remarks>
	/// <example>Test-Stepper-.ps1, "Test-Stepper+.ps1", "Test-Dialog+.ps1".</example>
	public sealed class Stepper // _100411_022932
	{
		/// <summary>
		/// The only allowed running instance.
		/// </summary>
		internal static Stepper RunningInstance { get { return Stepper._RunningInstance; } }
		static Stepper _RunningInstance;

		// Units and steps
		List<ScriptBlock> _units = new List<ScriptBlock>();
		ArrayList _steps = new ArrayList();

		/// <summary>
		/// Creates a stepper.
		/// </summary>
		public Stepper()
		{ }

		/// <summary>
		/// Total count of steps processed so far.
		/// </summary>
		public int StepCount { get { return _StepCount; } }
		int _StepCount;

		/// <summary>
		/// Total count of units processed so far.
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

		static bool _AskDefault;
		/// <summary>
		/// Default initial <see cref="Ask"/> value.
		/// </summary>
		public static bool AskDefault
		{
			get { return _AskDefault; }
			set { _AskDefault = value; }
		}

		bool _Ask = _AskDefault;
		/// <summary>
		/// Tells to ask for actions before each step.
		/// </summary>
		public bool Ask
		{
			get { return _Ask; }
			set { _Ask = value; }
		}

		/// <summary>
		/// Adds the step sequence and posts the first step or inserts the sequence just after the step being invoked.
		/// </summary>
		/// <param name="steps">Script blocks and key sequences.</param>
		/// <remarks>
		/// If it is called in order to start the sequence then normally it should be the last command in a script.
		/// </remarks>
		/// <seealso cref="Actor.Go"/>
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
		void InsertRange(int index, ICollection steps)
		{
			// state
			State = StepperState.Parsing;

			// steps
			foreach (object o in steps)
			{
				object oInsert;
				ScriptBlock b = Cast<ScriptBlock>.From(o);
				if (b != null)
				{
					oInsert = b;
				}
				else
				{
					string s = Cast<string>.From(o);
					if (s != null)
					{
						try
						{
							oInsert = Far.Net.CreateKeySequence(s);
							if (_Ask)
								oInsert = s;
						}
						catch (ArgumentException ex)
						{
							throw new InvalidOperationException("Invalid keys: '" + s + "'. " + ex.Message);
						}
					}
					else if (o == null)
					{
						throw new RuntimeException("Step sequence contains a null object.");
					}
					else
					{
						throw new RuntimeException("Step sequence contains unknown object: " + o.ToString());
					}
				}
				_steps.Insert(index, oInsert);
				++index;
			}
		}

		/// <summary>
		/// Posts a step unit command.
		/// </summary>
		/// <param name="command">Step unit command: text code or a script block.</param>
		/// <remarks>
		/// Step unit command is any command that returns steps, for example a step unit script path.
		/// <para>
		/// The command is posted to the end of the internal queue.
		/// Note that the order of posted units may be significant for the results.
		/// </para>
		/// </remarks>
		public void PostUnit(object command)
		{
			// null
			if (command == null)
				throw new ArgumentNullException("command");

			// code
			string code = Cast<string>.From(command);
			if (code != null)
			{
				_units.Add(ScriptBlock.Create(code));
				return;
			}

			// script
			ScriptBlock script = Cast<ScriptBlock>.From(command);
			if (script != null)
			{
				_units.Add(script);
				return;
			}

			// unknown
			throw new ArgumentException("'command' should be a string or a script block.");
		}

		/// <summary>
		/// Starts posted steps and step units processing.
		/// </summary>
		/// <remarks>
		/// Normally this should be the last command in the script.
		/// If step processing is already in progress then this call is ignored.
		/// </remarks>
		public void Go()
		{
			AssumeCanStep();

			if (_RunningInstance == null)
				Far.Net.PostStep(Action);
		}

		/// <summary>
		/// Current unit script block if any or null.
		/// </summary>
		public ScriptBlock CurrentUnit
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
				throw new InvalidOperationException("Stepper is already running, concurrent stepper is not allowed.");
		}

		/// <summary>
		/// Event is triggered when the stepper state has changed.
		/// </summary>
		/// <remarks>
		/// It is designed mostly for logging and monitoring tasks.
		/// Other scenarios are not recommended.
		/// <para>
		/// New state is exposed for a script handler as <c>$this.State</c>
		/// which is the <see cref="State"/> property of the stepper.
		/// </para>
		/// </remarks>
		public event EventHandler StateChanged;

		StepperState _state_ = StepperState.None;
		/// <summary>
		/// Current state. See <see cref="StateChanged"/> event.
		/// </summary>
		public StepperState State
		{
			get { return _state_; }
			private set
			{
				// the same?
				if (value == _state_)
					return;

				// change
				_state_ = value;

				// trigger
				if (StateChanged != null)
					StateChanged(this, null);
			}
		}

		Exception _Error;
		/// <summary>
		/// The error that has stopped the processing.
		/// </summary>
		/// <remarks>
		/// This error is usually a PowerShell error which is also kept as the <c>$Error[0]</c> item.
		/// Note that this is not always the case and the error may be not stored in the <c>$Error</c> list at all.
		/// </remarks>
		public Exception Error
		{
			get { return _Error; }
		}

		// Invokes the next step in the step sequence.
		void Action(object sender, EventArgs e)
		{
			AssumeCanStep();

			try
			{
				// this is running
				_RunningInstance = this;

				// done with steps?
				if (_StepIndex >= _steps.Count)
				{
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
					Collection<PSObject> steps = _units[_UnitIndex].Invoke();

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
					throw new RuntimeException("Step object is null.");

				// show
				if (_Ask)
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
						MsgOptions.LeftAligned | MsgOptions.Down,
						new string[] { "Step", "Continue", "Cancel" }))
					{
						case 0:
							break;
						case 1:
							_Ask = false;
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
					object value;
					try
					{
						value = block.InvokeReturnAsIs();
					}
					catch (RuntimeException ex)
					{
						throw new ModuleException("Step failed: " + ex.Message, ex);
					}

					// extra script, normally starts modal UI
					ScriptBlock script = Cast<ScriptBlock>.From(value);
					if (script != null)
					{
						++_StepIndex;
						Far.Net.PostStepAfterStep(Wrap.EventHandler(script), Action);
						return;
					}

					// extra keys, normally start modal UI
					string keys = Cast<string>.From(value);
					if (keys != null)
					{
						++_StepIndex;
						Far.Net.PostStepAfterKeys(keys, Action);
						return;
					}
				}
				else
				{
					// get keys
					int[] keys = Cast<int[]>.From(it);
					if (keys == null)
						keys = Far.Net.CreateKeySequence(it.ToString());

					// post keys
					Far.Net.PostKeySequence(keys);
				}

				// post
				++_StepIndex;
				Far.Net.PostStep(Action);
			}
			catch(Exception error)
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

	}
}
