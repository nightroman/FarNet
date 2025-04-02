using System.Collections;
using System.Management.Automation;

namespace PowerShellFar;

sealed class VarDictionary(PSVariableIntrinsics variable) : IDictionary
{
	readonly PSVariableIntrinsics _variable = variable;

	public object? this[object key]
	{
		get => _variable.GetValue(key.ToString());
		set => _variable.Set(key.ToString(), value);
	}

	public bool IsFixedSize => false;

	public bool IsReadOnly => false;

	public ICollection Keys => Array.Empty<object>();

	public ICollection Values => Array.Empty<object>();

	public int Count => 0;

	public bool IsSynchronized => false;

	public object SyncRoot => this;

	public void Add(object key, object? value)
	{
	}

	public void Clear()
	{
	}

	// Always called on $Var.name, kind of waste of time in normal mode.
	// So just return true and lose strict mode checks, not a big deal.
	public bool Contains(object key)
	{
		return true;
	}

	public void CopyTo(Array array, int index)
	{
	}

	// e.g. on `Get-Variable -Scope 0 | Out-String`
	public IDictionaryEnumerator GetEnumerator()
	{
		return new Hashtable().GetEnumerator();
	}

	public void Remove(object key)
	{
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
