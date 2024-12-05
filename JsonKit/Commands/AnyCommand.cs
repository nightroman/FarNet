using System;

namespace JsonKit.Commands;

abstract class AnyCommand : IDisposable
{
	public abstract void Invoke();

	public void Dispose()
	{
		Dispose(true);
	}

	protected virtual void Dispose(bool disposing)
	{
	}
}
