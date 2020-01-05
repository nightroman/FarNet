
// Copyright (c) Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

namespace HtmlToFarHelp
{
	enum ListKind
	{
		Ordered,
		Unordered,
		Definition
	}

	class ListInfo
	{
		public ListKind Kind { get; private set; }
		public int Item;
		public int ItemCount;
		public int TermCount;
		public int CountParaInItem;
		public ListInfo(ListKind kind)
		{
			Kind = kind;
		}
	}
}
