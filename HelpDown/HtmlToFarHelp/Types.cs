
// Copyright 2012-2016 Roman Kuzmin
// http://www.apache.org/licenses/LICENSE-2.0

namespace HtmlToFarHelp
{
	enum ListKind
	{
		None,
		Ordered,
		Unordered,
		Definition
	}

	struct Options
	{
		public static Options New()
		{
			return new Options()
			{
				Margin = 1,
				IndentCode = 4,
				IndentList = 2,
				IndentQuote = 4,
				Language = "English,English",
			};
		}
		public bool CenterHeading;
		public bool PlainCode;
		public bool PlainHeading;
		public int Margin;
		public int IndentCode;
		public int IndentList;
		public int IndentPara;
		public int IndentQuote;
		public string Language;
		public string PluginContents;
	}
}
