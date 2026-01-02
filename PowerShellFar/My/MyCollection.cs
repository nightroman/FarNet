using System.Collections;

namespace PowerShellFar;

abstract class MyCollection : ICollection
{
	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public void CopyTo(Array array, int index)
	{
		ArgumentNullException.ThrowIfNull(array);
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		if (Count > array.Length - index)
			throw new ArgumentException("Not enough space in the array.");

		foreach (var value in this)
		{
			array.SetValue(value, index);
			++index;
		}
	}

	public abstract int Count { get; }

	public abstract IEnumerator GetEnumerator();
}
