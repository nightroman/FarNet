
// http://www.codeproject.com/KB/threads/AbortIfSafe.aspx

using System;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;

namespace Pfz.Threading
{
	/// <summary>
	/// Class that allows thread-aborts to be done in a relativelly safe manner.
	/// </summary>
	static class SafeAbort
	{
		/// <summary>
		/// Aborts a thread only if it is safe to do so, taking the abort mode into account (which may
		/// range from only guaranteeing that IDisposable objects will be fully constructed, up to 
		/// guaranteeing all "using" blocks to work and even doing some user validations).
		/// Returns if the Thread.Abort() was called or not.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Threading.Thread.Resume")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Threading.Thread.Suspend")]
		public static bool AbortIfSafe(Thread thread, SafeAbortMode mode = SafeAbortMode.RunAllValidations, object stateInfo = null)
		{
			if (thread == null)
				throw new ArgumentNullException("thread");

			// If for some reason we are trying to abort our actual thread, we simple call Thread.Abort() directly.
			if (thread == Thread.CurrentThread)
				thread.Abort(stateInfo);

			// We check the state of the thread, ignoring if the thread also has Suspended or SuspendRequested in its state.
			switch (thread.ThreadState & ~(ThreadState.Suspended | ThreadState.SuspendRequested))
			{
				case ThreadState.Running:
				case ThreadState.Background:
				case ThreadState.WaitSleepJoin:
					break;

				case ThreadState.Stopped:
				case ThreadState.StopRequested:
				case ThreadState.AbortRequested:
				case ThreadState.Aborted:
					return true;

				default:
					throw new ThreadStateException("The thread is in an invalid state to be aborted.");
			}

			try
			{
#pragma warning disable
				thread.Suspend();
#pragma warning restore
			}
			catch (ThreadStateException)
			{
				switch (thread.ThreadState & ~(ThreadState.Suspended | ThreadState.SuspendRequested))
				{
					case ThreadState.Aborted:
					case ThreadState.Stopped:
						// The thread terminated just when we are trying to Abort it. So, we will return true to tell that the "Abort" succeeded.
						return true;
				}

				// we couldn't discover why Suspend threw an exception, so we rethrow it.
				throw;
			}

			// We asked the thread to suspend, but the thread may take some time to be really suspended, so we will wait until it is no more in "SuspendRequested" state.
			while (thread.ThreadState == ThreadState.SuspendRequested)
				Thread.Sleep(1);

			// The Thread ended just when we asked it to suspend. So, for us, the Abort succeeded.
			if ((thread.ThreadState & (ThreadState.Stopped | ThreadState.Aborted)) != ThreadState.Running)
				return true;

			try
			{
				var stack = new System.Diagnostics.StackTrace(thread, false);
				var frames = stack.GetFrames();

				// If we try to Abort the thread when it is starting (really soon), it will not have any frames. Calling an abort here caused some dead-locks for me,
				// so I consider that a Thread with no frames is a thread that can't be aborted.
				if (frames == null)
					return false;

				bool? canAbort = null;
				// In the for block, we start from the oldest frame to the newest one.
				// In fact, we check this: If the method returns IDisposable, then we can't abort. If this is not the case, then if we are inside a catch or finally
				// block, we can abort, as such blocks delay the normal abort and, so, even if an internal call is inside a constructor of an IDisposable, we
				// will not cause any problem calling abort. That's the reason to start with the oldest frame to the newest one.
				// And finally, if we are not in a problematic frame or in a guaranteed frame, we check if the method has try blocks. If it has, we consider
				// we can't abort. Note that if you do a try/catch and then an infinite loop, this check will consider the method to be inabortable.
				for (int i = frames.Length - 1; i >= 0; i--)
				{
					var frame = frames[i];
					var method = frame.GetMethod();

					// if we are inside a constructor of an IDisposable object or inside a function that returns one, we can't abort.
					if (method.IsConstructor)
					{
						ConstructorInfo constructorInfo = (ConstructorInfo)method;
						if (typeof(IDisposable).IsAssignableFrom(constructorInfo.DeclaringType))
						{
							canAbort = false;
							break;
						}
					}
					else
					{
						MethodInfo methodInfo = (MethodInfo)method;
						if (typeof(IDisposable).IsAssignableFrom(methodInfo.ReturnType))
						{
							canAbort = false;
							break;
						}
					}

					// Checks if the method, its class or its assembly has HostProtectionAttributes with MayLeakOnAbort.
					// If that's the case, then we can't abort.
					var attributes = (HostProtectionAttribute[])method.GetCustomAttributes(typeof(HostProtectionAttribute), false);
					foreach (var attribute in attributes)
					{
						if (attribute.MayLeakOnAbort)
						{
							canAbort = false;
							break;
						}
					}
					attributes = (HostProtectionAttribute[])method.DeclaringType.GetCustomAttributes(typeof(HostProtectionAttribute), false);
					foreach (var attribute in attributes)
					{
						if (attribute.MayLeakOnAbort)
						{
							canAbort = false;
							break;
						}
					}
					attributes = (HostProtectionAttribute[])method.DeclaringType.Assembly.GetCustomAttributes(typeof(HostProtectionAttribute), false);
					foreach (var attribute in attributes)
					{
						if (attribute.MayLeakOnAbort)
						{
							canAbort = false;
							break;
						}
					}

					var body = method.GetMethodBody();
					if (body == null)
						continue;

					// if we were inside a finally or catch, we can abort, as the normal Thread.Abort() will be naturally delayed.
					int offset = frame.GetILOffset();
					foreach (var handler in body.ExceptionHandlingClauses)
					{
						int handlerOffset = handler.HandlerOffset;
						int handlerEnd = handlerOffset + handler.HandlerLength;

						if (offset >= handlerOffset && offset < handlerEnd)
						{
							canAbort = true;
							break;

						}

						if (canAbort.GetValueOrDefault())
							break;
					}
				}

				if (canAbort == null)
				{
					if (mode == SafeAbortMode.AllowUsingsToFail)
						canAbort = true;
					else
					{
						// we are inside an unsure situation. So, we will try to check the method.
						var frame = frames[0];
						var method = frame.GetMethod();
						var body = method.GetMethodBody();
						if (body != null)
						{
							var handlingClauses = body.ExceptionHandlingClauses;
							if (handlingClauses.Count == 0)
							{
								canAbort = true;

								// Ok, by our tests we can abort. But, if the mode is RunAllValidations and there are user-validations, we must
								// run them.
								if (mode == SafeAbortMode.RunAllValidations)
								{
									var handler = Validating;
									if (handler != null)
									{
										SafeAbortEventArgs args = new SafeAbortEventArgs(thread, stack, frames);
										handler(null, args);

										// The args by default has its CanAbort set to true. But, if any handler changed it, we will not be able to abort.
										canAbort = args.CanAbort;
									}
								}
							}
						}
					}
				}

				if (canAbort.GetValueOrDefault())
				{
					try
					{
						// We need to call abort while the thread is suspended, that works, but causes an exception, so we ignore it.
						thread.Abort(stateInfo);
					}
					catch
					{ }

					return true;
				}

				return false;
			}
			finally
			{
#pragma warning disable
				thread.Resume();
#pragma warning restore
			}
		}
		/// <summary>
		/// Aborts a thread, trying to use the safest abort mode, until the unsafest one.
		/// The number of retries is also the expected number of milliseconds trying to abort.
		/// </summary>
		public static bool Abort(Thread thread, int triesWithAllValidations, int triesIgnoringUserValidations, int triesAllowingUsingsToFail, bool finalizeWithNormalAbort = false, object stateInfo = null)
		{
			if (thread == null)
				throw new ArgumentNullException("thread");

			for (int i = 0; i < triesWithAllValidations; i++)
			{
				if (AbortIfSafe(thread, SafeAbortMode.RunAllValidations, stateInfo))
					return true;

				Thread.Sleep(1);
			}

			for (int i = 0; i < triesIgnoringUserValidations; i++)
			{
				if (AbortIfSafe(thread, SafeAbortMode.IgnoreUserValidations, stateInfo))
					return true;

				Thread.Sleep(1);
			}

			for (int i = 0; i < triesAllowingUsingsToFail; i++)
			{
				if (AbortIfSafe(thread, SafeAbortMode.AllowUsingsToFail, stateInfo))
					return true;

				Thread.Sleep(1);
			}

			if (finalizeWithNormalAbort)
			{
				thread.Abort(stateInfo);
				return true;
			}

			return false;
		}
		/// <summary>
		/// Event invoked by AbortIfSafe if user validations are valid and when it is unsure if the thread
		/// is in a safe situation or not.
		/// </summary>
		public static event EventHandler<SafeAbortEventArgs> Validating;
	}
}
