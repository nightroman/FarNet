using System.Collections;

namespace PowerShellFar;

/// <summary>
/// Enumerator of T from S.
/// </summary>
/// <remarks>
/// Override <see cref="MoveNext"/>.
/// </remarks>
/// <typeparam name="S">Source type.</typeparam>
/// <typeparam name="T">Result type.</typeparam>
abstract class MyEnumerator<T, S>(IEnumerable<S> enumerable) : IEnumerator<T>
{
	protected IEnumerator<S> _enumerator = enumerable.GetEnumerator();
	protected T _current = default!;

	/// <summary>
	/// Calls _enumerator.MoveNext() and sets _current.
	/// </summary>
	abstract public bool MoveNext();

	public T Current => _current;

	object IEnumerator.Current => _current!;

	public void Reset() => _enumerator.Reset();

	public void Dispose()
	{
		_enumerator.Dispose();
		GC.SuppressFinalize(this);
	}
}
