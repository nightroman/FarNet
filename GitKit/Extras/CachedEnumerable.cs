namespace System.Collections.Generic;

// https://stackoverflow.com/a/34633464/323582
sealed class CachedEnumerable<T> : IEnumerable<T>, IDisposable
{
	readonly List<T> _cache = new();
	IEnumerator<T>? _enumerator;

	public CachedEnumerable(IEnumerable<T> enumerable)
	{
		_enumerator = enumerable.GetEnumerator();
	}

	public IEnumerator<T> GetEnumerator()
	{
		int index = 0;

		for (; index < _cache.Count; index++)
		{
			yield return _cache[index];
		}

		for (; _enumerator != null && _enumerator.MoveNext(); index++)
		{
			var current = _enumerator.Current;
			_cache.Add(current);
			yield return current;
		}

		if (_enumerator != null)
		{
			_enumerator.Dispose();
			_enumerator = null;
		}
	}

	public void Dispose()
	{
		if (_enumerator != null)
		{
			_enumerator.Dispose();
			_enumerator = null;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
