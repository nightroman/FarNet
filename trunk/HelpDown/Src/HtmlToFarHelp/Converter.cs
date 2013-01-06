
/* Copyright 2012-2013 Roman Kuzmin
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

namespace HtmlToFarHelp
{
	class Converter
	{
		const string ArgWrap = "§¦";
		const string ErrExpectedA = "Expected <a href=...>...</a>.";
		const string ErrInvalidHtml1 = "Invalid or not supported HTML: '{0}'.";
		const string ErrInvalidHtml2 = "Invalid or not supported HTML: '{0}'. At line {1} char {2}.";
		const string ErrMissingTarget = "Missing href target: {0}.";
		const string ErrNestedList = "Nested list is not supported.";
		const string ErrPreCode = "Expected <pre><code>...</code></pre>.";
		const string ErrTwoTopics = "The topic id '{0}' is used twice.";
		const string ErrUnexpectedElement = "Unexpected element '{0}'.";
		const string ErrUnexpectedNode = "Unexpected node {0} {1}.";
		readonly Regex _reNewLine = new Regex(@"\r?\n");
		readonly Regex _reSpaces = new Regex(" +");
		readonly Regex _reUnindent = new Regex(@"\n[\ \t]+");
		readonly Regex _reOptions = new Regex(@"^\s*HLF:\s*(.*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		readonly char[] TrimNewLine = new char[] { '\r', '\n' };
		readonly HashSet<string> _topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		readonly HashSet<string> _links = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		bool _needNewLine;
		int _emphasis;
		int _countParaInItem;
		int _countTextInPara;
		int _heading;
		int _item;
		int _itemCount;
		int _list;
		int _para;
		int _quote;
		int _termCount;
		ListKind _listKind;
		string _IndentCode_;
		string IndentCode { get { return _quote == 0 ? _IndentCode_ : _IndentCode_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentCode2_;
		string IndentCode2 { get { return _quote == 0 ? _IndentCode2_ : _IndentCode2_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentCode3_;
		string IndentCode3 { get { return _quote == 0 ? _IndentCode3_ : _IndentCode3_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentList_;
		string IndentList { get { return _quote == 0 ? _IndentList_ : _IndentList_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		string _IndentPara_;
		string IndentPara { get { return _quote == 0 ? _IndentPara_ : _IndentPara_ + "".PadRight(_quote * _options.IndentQuote, ' '); } }
		Options _globalOptions;
		Options _options;
		public XmlReader Reader { get; set; }
		public StreamWriter Writer { get; set; }
		static Options ParseOptions(Options options, string optionString)
		{
			try
			{
				var builder = new DbConnectionStringBuilder();
				builder.ConnectionString = optionString;

				foreach (DictionaryEntry it in (IDictionary)builder)
				{
					var value = it.Value.ToString();
					switch (it.Key.ToString())
					{
						case "centerheading": options.CenterHeading = bool.Parse(value); break;
						case "centerrule": options.CenterRule = bool.Parse(value); break;
						case "plaincode": options.PlainCode = bool.Parse(value); break;
						case "plainheading": options.PlainHeading = bool.Parse(value); break;
						case "margin": options.Margin = int.Parse(value); break;
						case "indentcode": options.IndentCode = int.Parse(value); break;
						case "indentlist": options.IndentList = int.Parse(value); break;
						case "indentpara": options.IndentPara = int.Parse(value); break;
						case "indentquote": options.IndentQuote = int.Parse(value); break;
						case "language": options.Language = value; break;
						default: throw new ArgumentException("Unknown option: " + it.Key);
					}
				}
			}
			catch (Exception e)
			{
				throw new InvalidDataException("Error on parsing HLF options: " + e.Message, e);
			}

			return options;
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
			if (Reader == null || Writer == null) throw new InvalidOperationException();

			// options
			_globalOptions = Options.New();
			_options = _globalOptions;
			ProcessOptions();

			// header
			Writer.WriteLine(".Language=" + _options.Language);
			Writer.WriteLine(".Options CtrlStartPosChar=" + ArgWrap);

			// parse
			while (Reader.Read())
			{
				switch (Reader.NodeType)
				{
					case XmlNodeType.Comment: Comment(); break;
					case XmlNodeType.Element: Element(); break;
					case XmlNodeType.EndElement: EndElement(); break;
					case XmlNodeType.Text: Text(); break;
					case XmlNodeType.Whitespace: Whitespace(); break;
					default:
						Throw(string.Format(ErrUnexpectedNode, Reader.NodeType, Reader.Name));
						break;
				}
			}

			// validate links
			foreach (var link in _links)
			{
				if (!_topics.Contains(link))
					throw new InvalidDataException(string.Format(ErrMissingTarget, link));
			}
		}
		void Comment()
		{
			var match = _reOptions.Match(Reader.Value);
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
				_options = ParseOptions(_options, text);
			}
			else
			{
				// make the global and current the same
				_globalOptions = ParseOptions(_globalOptions, text);
				_options = _globalOptions;
			}

			// apply
			ProcessOptions();
		}
		void NewLine()
		{
			if (_needNewLine)
			{
				Writer.WriteLine();
				_needNewLine = false;
			}
		}
		static string Escape(string text)
		{
			return text.Replace("#", "##").Replace("@", "@@").Replace("~", "~~");
		}
		void Whitespace()
		{
			if (_reSpaces.IsMatch(Reader.Value) || _para > 0)
				Writer.Write(Reader.Value);
		}
		void Throw(string text)
		{
			var textReader = Reader as XmlTextReader;

			string message;
			if (textReader == null || !textReader.HasLineInfo())
				message = string.Format(ErrInvalidHtml1, text);
			else
				message = string.Format(ErrInvalidHtml2, text, textReader.LineNumber, textReader.LinePosition);

			throw new InvalidDataException(message);
		}
		void Element()
		{
			switch (Reader.Name)
			{
				case "a": A1(); break;
				case "body": break;
				case "blockquote": Quote1(); break;
				case "br": break;
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
				case "h6": Heading1(); break;
				case "head": Reader.Skip(); break;
				case "hr": Rule(); break;
				case "html": break;
				case "li": Item1(); break;
				case "ol": List1(ListKind.Ordered); break;
				case "p": P1(); break;
				case "pre": Pre(); break;
				case "strong": Emphasis1(); break;
				case "title": Reader.Skip(); break;
				case "ul": List1(ListKind.Unordered); break;
				default:
					Throw(string.Format(ErrUnexpectedElement, Reader.Name));
					break;
			}
		}
		void EndElement()
		{
			switch (Reader.Name)
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
				case "h6": Heading2(); break;
				case "li": Item2(); break;
				case "ol": List2(); break;
				case "p": P2(); break;
				case "strong": Emphasis2(); break;
				case "ul": List2(); break;
			}
		}
		void A1()
		{
			string href = Reader.GetAttribute("href");
			if (href == null) Throw(ErrExpectedA);

			if (href.StartsWith("#"))
			{
				href = href.Substring(1);
				_links.Add(href);
			}

			Reader.MoveToContent();
			Reader.Read();
			if (Reader.NodeType != XmlNodeType.Text)
				Throw(ErrExpectedA);

			var text = Reader.Value;
			Writer.Write("~{0}~@{1}@", Escape(text), href.Replace("@", "@@"));
		}
		void Heading1()
		{
			++_heading;
			Writer.WriteLine();
			Writer.WriteLine();

			string id = Reader.GetAttribute("id");
			if (id == null)
			{
				if (_options.CenterHeading)
					Writer.Write("^");
				else
					Writer.Write(IndentPara);
			}
			else
			{
				if (!_topics.Add(id))
					Throw(string.Format(ErrTwoTopics, id));

				Writer.WriteLine("@{0}", id);
				if (_options.CenterHeading)
					Writer.Write("$^");
				else
					Writer.Write("${0}", IndentPara);
			}

			if (!_options.PlainHeading)
			{
				Writer.Write("#");
				++_emphasis;
			}
		}
		void Heading2()
		{
			_heading = 0;
			_emphasis = 0;

			if (!_options.PlainHeading)
				Writer.Write("#");
		}
		void Emphasis1()
		{
			++_emphasis;

			NewLine();

			if (_emphasis == 1)
				Writer.Write("#");
		}
		void Emphasis2()
		{
			--_emphasis;

			if (_emphasis == 0)
				Writer.Write("#");
		}
		void Term1()
		{
			++_termCount;

			Writer.WriteLine();
			if (_termCount > 1)
				Writer.WriteLine();

			_countParaInItem = 0;
			Writer.Write(IndentPara);
		}
		void Item1()
		{
			++_item;
			++_itemCount;
			_needNewLine = false;
			_countTextInPara = 0;

			Writer.WriteLine();
			if (_countParaInItem > 0 && _listKind != ListKind.Definition)
				Writer.WriteLine();

			_countParaInItem = 0;

			Writer.Write(IndentList);
			switch (_listKind)
			{
				case ListKind.Ordered:
					Writer.Write("{0}. " + ArgWrap, _itemCount);
					break;
				case ListKind.Unordered:
					Writer.Write("• " + ArgWrap);
					break;
				case ListKind.Definition:
					Writer.Write("  " + ArgWrap);
					break;
			}
		}
		void Item2()
		{
			--_item;
		}
		void List1(ListKind kind)
		{
			++_list;
			if (_list > 1)
				Throw(ErrNestedList);

			_listKind = kind;
			_itemCount = 0;
			_termCount = 0;

			Writer.WriteLine();
		}
		void List2()
		{
			--_list;
			_countParaInItem = 0;
			_listKind = ListKind.None;
		}
		void Rule()
		{
			Writer.WriteLine();
			Writer.WriteLine();

			if (_options.CenterRule)
				Writer.Write("^");

			Writer.Write("___________________________________________________________________");
		}
		void P1()
		{
			++_para;
			_needNewLine = false;
			_countTextInPara = 0;

			if (_item > 0)
				++_countParaInItem;

			if (_item == 0)
			{
				Writer.WriteLine();
				Writer.WriteLine();

				Writer.Write(IndentPara);
			}
			else if (_countParaInItem > 1)
			{
				Writer.WriteLine();
				Writer.WriteLine();

				if (_listKind == ListKind.Ordered)
					Writer.Write(IndentList + "   " + ArgWrap);
				else
					Writer.Write(IndentList + "  " + ArgWrap);
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
			Reader.Read();
			if (Reader.NodeType == XmlNodeType.Whitespace)
				Reader.Read();
			if (Reader.NodeType != XmlNodeType.Element || Reader.Name != "code")
				Throw(ErrPreCode);

			var code = Reader.ReadElementContentAsString().Trim();
			var lines = code.Split('\n');

			Writer.WriteLine();
			Writer.WriteLine();
			bool newLine = false;
			string indent = _list == 0 ? IndentCode : _listKind == ListKind.Ordered ? IndentCode3 : IndentCode2;
			foreach (var line in lines)
			{
				if (newLine)
					Writer.WriteLine();
				else
					newLine = true;

				if (line.Length > 0)
				{
					var text = indent + Escape(line.TrimEnd());
					if (_options.PlainCode)
						Writer.Write(text);
					else
						Writer.Write(" #" + text + "#");
				}
			}
		}
		void Text()
		{
			++_countTextInPara;
			var text = Reader.Value;

			NewLine();

			// trim new lines
			if (_list > 0)
			{
				var len1 = text.Length;
				text = text.TrimStart(TrimNewLine);
				if (len1 != text.Length && _countTextInPara > 1)
					Writer.WriteLine();

				var len2 = text.Length;
				text = text.TrimEnd(TrimNewLine);
				_needNewLine = len2 != text.Length;
			}

			// unindent second+ lines, otherwise HLF treats them as new para
			if (_para > 0 || _list > 0)
				text = _reUnindent.Replace(text, string.Empty);

			// escape
			text = Escape(text);

			// add extra # to the line end and to the next line start
			if (_emphasis > 0)
				text = _reNewLine.Replace(text, "#\r\n#");

			Writer.Write(text);
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
