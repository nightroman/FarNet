#pragma once

namespace FarManagerImpl
{;
/// <summary>
/// Enumerator of ILine items in IList.
/// </summary>
public ref class LineListEnumerator : public IEnumerator<ILine^>
{
public:
	LineListEnumerator(IList<ILine^>^ list, int start, int end)
		: _list(list), _start(_start), _end(end), _index(start - 1)
	{}
	~LineListEnumerator()
	{}
	virtual property ILine^ Current
	{
		ILine^ get() = IEnumerator<ILine^>::Current::get
		{
			return _list[_index];
		}
	}
	virtual bool MoveNext()
	{
		return ++_index < _end;
	}
	virtual void Reset()
	{
		_index = _start;
	}
private:
	virtual property Object^ CurrentObject
	{
		Object^ get() sealed = System::Collections::IEnumerator::Current::get
		{
			return _list[_index];
		}
	}
private:
	IList<ILine^>^ _list;
	int _start;
	int _end;
	int _index;
};
}
