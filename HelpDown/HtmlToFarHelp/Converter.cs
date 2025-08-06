
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace HtmlToFarHelp
{
	class Converter
	{
		const string ArgWrap = "§¦";
		const string ErrExpectedA = "Expected <a href=...>...</a>.";
		const string ErrExpectedList = "Expected list.";
		const string ErrInvalidHtml1 = "Invalid or not supported HTML: {0} At {1}";
		const string ErrInvalidHtml2 = "Invalid or not supported HTML: {0} At {1}:{2}:{3}";
		const string ErrMissingTarget = "Missing href target: {0}.";
		const string ErrPreCode = "Expected <pre><code>...</code></pre>.";
		const string ErrTwoTopics = "The topic id '{0}' is used twice.";
		const string ErrUnexpectedElement = "Unexpected element '{0}'.";
		const string ErrUnexpectedNode = "Unexpected node {0} {1}.";
		readonly HashSet<string> _topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		readonly HashSet<string> _links = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		bool _br;
		bool _started;
		bool _needNewLine;
		int _emphasis;
		int _countTextInPara;
		int _para;
		int _quote;
		int _internalHeadings;
		string _lastNodeName;
		readonly Stack<ListInfo> _list = new Stack<ListInfo>();
		string _IndentCode_;
		string IndentCode { get { return _quote == 0 ? _IndentCode_ : _IndentCode_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentCode2_;
		string IndentCode2 { get { return _quote == 0 ? _IndentCode2_ : _IndentCode2_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentCode3_;
		string IndentCode3 { get { return _quote == 0 ? _IndentCode3_ : _IndentCode3_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentList_;
		string IndentList
		{
			get
			{
				var r = _quote == 0 ? _IndentList_ : _IndentList_ + "".PadRight(_quote * _options.IndentQuote, ' ');
				if (_list.Count > 1)
					return "".PadRight(4 * (_list.Count - 1), ' ') + r;
				else
					return r;
			}
		}
		string _IndentPara_;
		string IndentPara { get { return _quote == 0 ? _IndentPara_ : _IndentPara_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		Options _globalOptions;
		Options _options;
		readonly string _fileName;
		readonly XmlReader _reader;
		readonly StreamWriter _writer;
		readonly bool _verbose;

		public Converter(string inputFileName, XmlReader reader, StreamWriter writer, bool verbose)
		{
			_fileName = inputFileName;
			_reader = reader;
			_writer = writer;
			_verbose = verbose;
		}

		void ProcessOptions()
		{
			_IndentList_ = "".PadRight(_options.Margin + _options.IndentList, ' ');
			_IndentPara_ = "".PadRight(_options.Margin + _options.IndentPara, ' ');

			int indentCode = _options.IndentCode;
			if (!_options.PlainCode && indentCode > 0)
				--indentCode;

			_IndentCode_ = "".PadRight(_options.Margin + indentCode, ' ');
			_IndentCode2_ = _IndentList_ + "  " + "".PadRight(indentCode, ' ');
			_IndentCode3_ = _IndentList_ + "   " + "".PadRight(indentCode, ' ');
		}

		public void Run()
		{
			if (_reader == null || _writer == null) throw new InvalidOperationException();

			// options
			_globalOptions = Options.CreateDefault();
			_options = _globalOptions;
			ProcessOptions();

			// parse
			while (_reader.Read())
				Node();

			// validate links
			foreach (var link in _links)
			{
				if (!_topics.Contains(link))
					throw new InvalidDataException(string.Format(ErrMissingTarget, link));
			}
		}

		void Node()
		{
			switch (_reader.NodeType)
			{
				case XmlNodeType.Comment: Comment(); break;
				case XmlNodeType.Element: Element(); break;
				case XmlNodeType.EndElement: EndElement(); break;
				case XmlNodeType.Text: Text(); break;
				case XmlNodeType.Whitespace: Whitespace(); break;
				case XmlNodeType.DocumentType: break;
				default:
					Throw(string.Format(ErrUnexpectedNode, _reader.NodeType, _reader.Name));
					break;
			}
		}

		void Start()
		{
			if (_started)
				return;

			_started = true;

			_writer.WriteLine(".Language=" + _options.Language);

			if (_options.PluginContents != null)
				_writer.WriteLine(".PluginContents=" + _options.PluginContents);

			_writer.WriteLine(".Options CtrlStartPosChar=" + ArgWrap);
		}

		void Comment()
		{
			var match = Kit.MatchOptions(_reader.Value);
			if (!match.Success)
				return;

			var text = match.Groups[1].Value.TrimEnd();
			if (text.Length == 0)
			{
				// reset to the global
				_options = _globalOptions;
			}
			else if (_topics.Count > 0)
			{
				// update the current
				_options = Options.Parse(_options, text);
			}
			else
			{
				// make the global and current the same
				_globalOptions = Options.Parse(_globalOptions, text);
				_options = _globalOptions;
			}

			// apply
			ProcessOptions();
		}

		void NewLine()
		{
			if (_needNewLine)
			{
				_writer.WriteLine();
				_needNewLine = false;
			}
		}

		static string Escape(string text)
		{
			return text.Replace("#", "##").Replace("@", "@@").Replace("~", "~~");
		}

		void Whitespace()
		{
			if ((_para > 0 || Kit.HasSpaces(_reader.Value)) && !_br)
				_writer.Write(Kit.FixNewLine(_reader.Value));
		}

		void Throw(string text)
		{
			string message;
			if (_reader is IXmlLineInfo lineInfo && lineInfo.HasLineInfo())
				message = string.Format(ErrInvalidHtml2, text, _fileName, lineInfo.LineNumber, lineInfo.LinePosition);
			else
				message = string.Format(ErrInvalidHtml1, text, _fileName);

			throw new InvalidDataException(message);
		}

		void Element()
		{
			var nodeName = _reader.Name;
			switch (nodeName)
			{
				case "a": A1(); break;
				case "blockquote": Quote1(); break;
				case "br": BR(); break;
				case "code": Emphasis1(); break;
				case "dd": Item1(); break;
				case "dl": List1(ListKind.Definition); break;
				case "dt": Term1(); break;
				case "em": Emphasis1(); break;
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6": Heading1(_reader.Name); break;
				case "hr": Rule(); break;
				case "kbd": Emphasis1(); break;
				case "li": Item1(); break;
				case "ol": List1(ListKind.Ordered); break;
				case "p": P1(); break;
				case "pre": Pre(); break;
				case "strong": Emphasis1(); break;
				case "ul": List1(ListKind.Unordered); break;
				// break
				case "body":
				case "html":
				case "div":
					break;
				// skip
				case "head":
				case "script": // pandoc email
				case "title":
					_reader.Skip();
					break;
				default:
					Throw(string.Format(ErrUnexpectedElement, _reader.Name));
					break;
			}
			_lastNodeName = nodeName;
		}

		bool _isLastElementHeading;
		void EndElement()
		{
			_isLastElementHeading = false;
			switch (_reader.Name)
			{
				case "blockquote": Quote2(); break;
				case "code": Emphasis2(); break;
				case "dd": Item2(); break;
				case "dl": List2(); break;
				case "dt": break;
				case "em": Emphasis2(); break;
				case "h1":
				case "h2":
				case "h3":
				case "h4":
				case "h5":
				case "h6": Heading2(); _isLastElementHeading = true; break;
				case "kbd": Emphasis2(); break;
				case "li": Item2(); break;
				case "ol": List2(); break;
				case "p": P2(); break;
				case "strong": Emphasis2(); break;
				case "ul": List2(); break;
			}

			EndElement2();
		}

		// Some resets after processing end elements.
		void EndElement2()
		{
			//! Pandoc produces `<dd>XYZ\r\n</dd>` with unwanted `\r\n` which leaves _needNewLine=true.
			//! This affects the next item and gives `\r\n \r\nXYZ` instead of `\r\n XYZ`.
			//! Ultimate solution: reset _needNewLine on each EndElement.
			_needNewLine = false;

			// just convenient to reset here
			_br = false;
		}

		// Reads just text, https://github.com/nightroman/FarNet/issues/45
		string ReadText(string name, bool readWhitespace)
		{
			var sb = new StringBuilder();
			for (; ;_reader.Read() )
			{
				if (_reader.NodeType == XmlNodeType.Element)
				{
					_reader.MoveToContent();
					continue;
				}

				if (_reader.NodeType == XmlNodeType.Text)
				{
					sb.Append(_reader.Value);
					continue;
				}

				if (_reader.NodeType == XmlNodeType.Whitespace)
				{
					if (readWhitespace)
						sb.Append(Kit.FixNewLine(_reader.Value));
					continue;
				}

				if (_reader.NodeType == XmlNodeType.EndElement)
				{
					if (_reader.Name == name)
						break;

					continue;
				}

				throw new InvalidOperationException($"Unexpected node type '{_reader.NodeType}'.");
			}
			return sb.ToString();
		}

		void A1()
		{
			string href = _reader.GetAttribute("href");
			if (href == null)
				Throw(ErrExpectedA);

			if (href.StartsWith("#"))
			{
				href = href.Substring(1);
				if (href == _topicContentsId)
					href = "Contents";
				_links.Add(href);
			}

			var text = Kit.FixNewLine(ReadText("a", false));

			// (1) last written text could have the deferred end new line, write it, #58
			NewLine();

			// (2) write the link
			_writer.Write("~{0}~@{1}@", Escape(text), href.Replace("@", "@@").Replace("#", "##"));

			//! because normal EndElement is not called, we have read it
			EndElement2();
		}

		// https://github.com/nightroman/FarNet/issues/44
		void BR()
		{
			_br = true;
			_writer.WriteLine();

			var tryList = _list.Count == 0 ? null : _list.Peek();
			if (tryList == null || tryList.Item == 0)
			{
				_writer.Write(IndentPara);
			}
			else
			{
				if (tryList.Kind == ListKind.Ordered)
					_writer.Write(IndentList + "   " + ArgWrap);
				else
					_writer.Write(IndentList + "  " + ArgWrap);
			}
		}

		string _topicContentsId;

		void Heading1(string tag)
		{
			Start();

			var id = _reader.GetAttribute("id");
			if (_topicContentsId != null && (id == null || string.CompareOrdinal(tag, _options.TopicHeading) > 0))
			{
				// internal heading
				++_internalHeadings;

				// empty lines
				_writer.WriteLine();
				_writer.WriteLine();
				if (!_isLastElementHeading)
				{
					for (int n = _options.EmptyLinesBeforeHeading; n > 1; --n)
						_writer.WriteLine();
				}

				if (_options.CenterHeading)
					_writer.Write("^");
				else
					_writer.Write(IndentPara);
			}
			else
			{
				// new topic heading
				_internalHeadings = 0;

				if (_verbose)
					Console.Out.WriteLine($"HeadingId={id}");

				_writer.WriteLine();
				_writer.WriteLine();

				if (_topicContentsId == null)
				{
					// first topic becomes "Contents"
					_topicContentsId = id ?? "Contents";
					_topics.Add("Contents");
					_writer.WriteLine("@Contents");
				}
				else
				{
					// other topics
					if (!_topics.Add(id))
						Throw(string.Format(ErrTwoTopics, id));

					// empty lines
					for (int n = _options.EmptyLinesBeforeTopic; n > 1; --n)
						_writer.WriteLine();

					_writer.WriteLine("@{0}", id);
				}

				if (_options.CenterHeading)
					_writer.Write("$^");
				else
					_writer.Write("${0}", IndentPara);
			}

			if (!_options.PlainHeading)
			{
				_writer.Write("#");
				++_emphasis;
			}
		}

		void Heading2()
		{
			if (!_options.PlainHeading)
				_writer.Write("#");

			// empty lines
			if (_internalHeadings > 0)
			{
				for (int n = _options.EmptyLinesAfterHeading; n > 1; --n)
					_writer.WriteLine();
			}

			_emphasis = 0;
		}

		void Emphasis1()
		{
			++_emphasis;

			NewLine();

			if (_emphasis == 1)
				_writer.Write("#");
		}

		void Emphasis2()
		{
			--_emphasis;

			if (_emphasis == 0)
				_writer.Write("#");
		}

		void Term1()
		{
			if (_list.Count == 0)
				Throw(ErrExpectedList);

			var list = _list.Peek();
			++list.TermCount;

			_writer.WriteLine();
			if (list.TermCount > 1)
				_writer.WriteLine();

			list.CountParaInItem = 0;
			_writer.Write(IndentPara);
		}

		string GetListPrefixOrdered()
		{
			return _options.HighlightListNumber ? "#{0}.# " : "{0}. ";
		}

		string GetListPrefixUnordered()
		{
			string bullet;
			if (_options.ListBullet is null)
			{
				bullet = Options.DefaultBullet;
			}
			else
			{
				var index = _list.Count <= _options.ListBullet.Length ? _list.Count - 1 : _options.ListBullet.Length - 1;
				bullet = _options.ListBullet[index];
			}

			return _options.HighlightListBullet ? $"#{bullet}# " : $"{bullet} ";
		}

		void Item1()
		{
			if (_list.Count == 0)
				Throw(ErrExpectedList);

			var list = _list.Peek();
			++list.Item;
			++list.ItemCount;
			_needNewLine = false;
			_countTextInPara = 0;

			_writer.WriteLine();

			if ((list.CountParaInItem > 0 || _options.ListItemEmptyLine) && list.Kind != ListKind.Definition)
				_writer.WriteLine();

			list.CountParaInItem = 0;

			_writer.Write(IndentList);
			switch (list.Kind)
			{
				case ListKind.Ordered:
					_writer.Write(GetListPrefixOrdered() + ArgWrap, list.ItemCount);
					break;
				case ListKind.Unordered:
					_writer.Write(GetListPrefixUnordered() + ArgWrap);
					break;
				case ListKind.Definition:
					_writer.Write("  " + ArgWrap);
					break;
			}
		}

		void Item2()
		{
			var list = _list.Peek();
			--list.Item;
		}

		void List1(ListKind kind)
		{
			if (_list.Count == 0 || _list.Peek().CountParaInItem > 0)
				_writer.WriteLine();

			_list.Push(new ListInfo(kind));
		}

		void List2()
		{
			_list.Pop();
			_needNewLine = false;
		}

		void Rule()
		{
			_writer.WriteLine();
			_writer.WriteLine();
			_writer.Write("@=");
		}

		void P1()
		{
			++_para;
			_needNewLine = false;
			_countTextInPara = 0;

			var tryList = _list.Count == 0 ? null : _list.Peek();
			if (tryList != null && tryList.Item > 0)
				++tryList.CountParaInItem;

			if (tryList == null || tryList.Item == 0)
			{
				_writer.WriteLine();
				_writer.WriteLine();

				_writer.Write(IndentPara);
			}
			else if (tryList.CountParaInItem > 1)
			{
				_writer.WriteLine();
				_writer.WriteLine();

				if (tryList.Kind == ListKind.Ordered)
					_writer.Write(IndentList + "   " + ArgWrap);
				else
					_writer.Write(IndentList + "  " + ArgWrap);
			}
		}

		void P2()
		{
			--_para;
			_needNewLine = false;
			_countTextInPara = 0;
		}

		void Pre()
		{
			_reader.Read();
			if (_reader.NodeType == XmlNodeType.Whitespace)
				_reader.Read();
			if (_reader.NodeType != XmlNodeType.Element || _reader.Name != "code")
				Throw(ErrPreCode);

			var code = ReadText("code", true).TrimEnd();
			var lines = Kit.TextToLines(code);

			_writer.WriteLine();
			_writer.WriteLine();
			bool newLine = false;
			var tryList = _list.Count == 0 ? null : _list.Peek();
			var indent = tryList == null ? IndentCode : tryList.Kind == ListKind.Ordered ? IndentCode3 : IndentCode2;
			foreach (var line in lines)
			{
				if (newLine)
					_writer.WriteLine();
				else
					newLine = true;

				if (line.Length > 0)
				{
					var text = indent + Escape(line.TrimEnd());
					if (_options.PlainCode)
						_writer.Write(text);
					else
						_writer.Write(" #" + text + "#");
				}
			}
		}

		void Text()
		{
			++_countTextInPara;
			var text = Kit.FixNewLine(_reader.Value);

			bool mayNeedSpace = text[0] == '\r' && !_needNewLine;
			NewLine();

			// trim new lines
			if (_list.Count > 0)
			{
				var len1 = text.Length;
				text = Kit.TrimStartNewLine(text);
				if (len1 != text.Length && _countTextInPara > 1 && !_br)
				{
					_writer.WriteLine();
					mayNeedSpace = false;
				}

				var len2 = text.Length;
				text = Kit.TrimEndNewLine(text);
				_needNewLine = len2 != text.Length;
			}

			// unindent second+ lines, otherwise HLF treats them as new para
			if (_para > 0 || _list.Count > 0)
				text = Kit.UnindentText(text);

			// escape
			text = Escape(text);

			// add extra # to the line end and to the next line start
			if (_emphasis > 0)
				text = Kit.EmphasisText(text);

			if (_br)
				text = Kit.TrimStartNewLine(text);

			// 2024-12-31-0758 pandoc without --wrap=preserve uses new line after <a> instead of space
			if (mayNeedSpace && !char.IsWhiteSpace(text[0]) && _lastNodeName == "a")
				_writer.Write(' ');

			_writer.Write(text);
		}

		void Quote1()
		{
			++_quote;
		}

		void Quote2()
		{
			--_quote;
		}
	}
}
