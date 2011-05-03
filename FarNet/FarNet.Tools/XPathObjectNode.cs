
// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace FarNet.Tools
{
	class XPathObjectNode
	{
		static readonly XmlAttributeInfo[] _emptyAttributes = new XmlAttributeInfo[0];
		static readonly XPathObjectNode[] _emptyElements = new XPathObjectNode[0];
		readonly XPathObjectContext _context;
		readonly object _target;
		readonly string _name;
		readonly XPathObjectNode _parent;
		int _index;
		IList<XmlAttributeInfo> _attributes;
		IList<XPathObjectNode> _elements;
		public object UnderlyingObject { get { return _target; } }
		public int Index { get { return _index; } }
		public XPathObjectNode(XPathObjectContext context, object target) : this(context, target, null, null, -1) { }
		XPathObjectNode(XPathObjectContext context, object target, string name, XPathObjectNode parent, int index)
		{
			if (context == null) throw new ArgumentNullException("context");
			if (target == null) throw new ArgumentNullException("target");

			_context = context;
			_target = target;
			_parent = parent;
			_index = index;

			if (string.IsNullOrEmpty(name))
			{
				var info = target as IXmlInfo;
				if (info == null)
				{
					var type = target.GetType();
					name = target.GetType().Name;
					if (type.IsGenericType)
						name = name.Remove(name.IndexOf('`'));
				}
				else
				{
					name = info.XmlNodeName();
				}
			}
			_name = GetAtomicString(name);
		}
		public string Name { get { return _name; } }
		public XPathObjectNode Parent { get { return _parent; } }
		public string Value
		{
			get
			{
				if (HasText)
					return CultureSafeToString(_target);

				return string.Empty;
			}
		}
		public bool HasAttributes
		{
			get
			{
				if (_attributes == null)
					ActivateAttributes();

				return _attributes.Count > 0;
			}
		}
		public bool HasChildren
		{
			get
			{
				if (_elements == null)
					ActivateElements();

				return _elements.Count > 0 || HasText;
			}
		}
		public bool HasText
		{
			get
			{
				Type type = _target.GetType();

				return (type.IsValueType || type == typeof(string));
			}
		}
		public IList<XmlAttributeInfo> Attributes
		{
			get
			{
				if (_attributes == null)
					ActivateAttributes();

				return _attributes;
			}
		}
		public string GetAttributeValue(string name)
		{
			if (_attributes == null)
				ActivateAttributes();

			foreach (var it in _attributes)
				if (it.Name == name)
					return CultureSafeToString(it.Getter(_target)) ?? string.Empty;

			return string.Empty;
		}
		public IList<XPathObjectNode> Elements
		{
			get
			{
				if (_elements == null)
					ActivateElements();

				return _elements;
			}
		}
		public void AddSpecialName(string key, string value)
		{
			if (_attributes == null)
				ActivateAttributes();

			// clone if read only
			if (_attributes.IsReadOnly)
				_attributes = new List<XmlAttributeInfo>(_attributes);

			_attributes.Add(new XmlAttributeInfo("*" + key, (object v) => value));
		}
		void ActivateAttributes() //?? lock was used, why?
		{
			if (_context.Stopping != null && _context.Stopping(null))
				return;

			if (_attributes != null)
				return;

			if (_target is ValueType || _target is string) //rvk "Linear types". Perhaps we need more.
			{
				// no attributes or children
				_attributes = _emptyAttributes;
				_elements = _emptyElements;
			}
			else if (_target is SuperFile)
			{
				ActivateSuperFileAttributes();
			}
			else if (_target is IDictionary) //! before ICollection
			{
				ActivateDictionary();
			}
			else if (_target is ICollection) //! after IDictionary
			{
				ActivateCollection();
			}
			else
			{
				ActivateSimple();
			}
		}
		void ActivateElements() //?? lock was used, why?
		{
			if (_context.Stopping != null && _context.Stopping(null))
				return;

			if (_elements != null)
				return;

			if (_target is ValueType || _target is string) //rvk "Linear types". Perhaps we need more.
			{
				// no attributes or children
				_attributes = _emptyAttributes;
				_elements = _emptyElements;
			}
			else if (_target is SuperFile)
			{
				ActivateSuperFileElements();
			}
			else if (_target is IDictionary) //! before ICollection
			{
				ActivateDictionary();
			}
			else if (_target is ICollection) //! after IDictionary
			{
				ActivateCollection();
			}
			else
			{
				ActivateSimple();
			}
		}
		void ActivateDictionary()
		{
			// no attributes
			_attributes = _emptyAttributes;

			// collect elements
			_elements = new List<XPathObjectNode>();

			foreach (DictionaryEntry entry in (IDictionary)_target)
			{
				if (entry.Value == null)
					continue;

				var node = new XPathObjectNode(_context, entry.Value, null, this, _elements.Count); //??????

				_elements.Add(node);

				node.AddSpecialName("key", entry.Key.ToString());
			}

			if (_elements.Count == 0)
				_elements = _emptyElements;
		}
		void ActivateCollection()
		{
			// no attributes
			_attributes = _emptyAttributes;

			// collect elements
			_elements = new List<XPathObjectNode>();
			foreach (object val in (ICollection)_target)
			{
				if (val == null)
					continue;

				_elements.Add(new XPathObjectNode(_context, val, null, this, _elements.Count));
			}

			if (_elements.Count == 0)
				_elements = _emptyElements;
		}
		void ActivateSimple()
		{
			var info = _target as IXmlInfo; //?????? need
			if (info != null)
			{
				_attributes = info.XmlAttributes();
				_elements = _emptyElements; //?????? elements
			}
			else
			{
				_attributes = new List<XmlAttributeInfo>();
				_elements = new List<XPathObjectNode>();

				foreach (PropertyInfo pi in _target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					// get the value
					object val = pi.GetValue(_target, null);

					if (val == null)
						continue;

					// get the custom attributes
					//rvk It is done just to skip XmlIgnoreAttribute. Do we need this expensive job?
					object[] attrs = pi.GetCustomAttributes(true);
					bool skip = false;

					if (attrs != null)
					{
						foreach (Attribute a in attrs)
						{
							if (a is System.Xml.Serialization.XmlIgnoreAttribute)
							{
								skip = true;
								break;
							}
						}
					}

					if (skip)
						continue; //rvk: It was break == bug?

					// now handle the values
					string str = CultureSafeToString(val);

					if (str != null)
						_attributes.Add(new XmlAttributeInfo(GetAtomicString(pi.Name), (object v) => str));
					else
						_elements.Add(new XPathObjectNode(_context, val, pi.Name, this, _elements.Count));
				}

				if (_elements.Count == 0)
					_elements = _emptyElements;
			}

			if (_attributes.Count == 0)
				_attributes = _emptyAttributes;
		}
		void ActivateSuperFileAttributes()
		{
			var file = (SuperFile)_target; //??????
			_attributes = file.XmlAttributes();
		}
		void ActivateSuperFileElements()
		{
			var file = (SuperFile)_target; //??????

			if (!file.IsDirectory)
			{
				_elements = _emptyElements;
				return;
			}

			// progress
			if (_context.IncrementDirectoryCount != null)
				_context.IncrementDirectoryCount(1);

			_elements = new List<XPathObjectNode>();

			Explorer explorer;
			if (file.Explorer.CanExploreLocation)
			{
				var argsExplore = new ExploreLocationEventArgs(ExplorerModes.Find, file.File.Name);
				explorer = file.Explorer.ExploreLocation(argsExplore);
			}
			else
			{
				var argsExplore = new ExploreDirectoryEventArgs(ExplorerModes.Find, file.File);
				explorer = file.Explorer.ExploreDirectory(argsExplore);
			}

			if (explorer != null)
			{
				var argsFiles = new GetFilesEventArgs(ExplorerModes.Find);
				foreach (var it in explorer.GetFiles(argsFiles))
				{
					// filter out a leaf
					if (_context.Filter != null && !it.IsDirectory && !_context.Filter(explorer, it))
						continue;

					// add
					_elements.Add(new XPathObjectNode(_context, new SuperFile(explorer, it), null, this, _elements.Count));
				}
			}

			if (_elements.Count == 0)
				_elements = _emptyElements;
		}
		string GetAtomicString(string array)
		{
			return _context.NameTable.Get(array) ?? _context.NameTable.Add(array);
		}
		internal static string CultureSafeToString(object value)
		{
			// string
			var asString = value as string;
			if (asString != null)
				return asString;

			if (value is ValueType)
			{
				// DateTime
				if (value is DateTime)
				{
					var asDateTime = (DateTime)value;
					return asDateTime.ToString((asDateTime.TimeOfDay.Ticks > 0 ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd"), null);
				}

				// Boolean
				if (value is bool)
				{
					var asBool = (bool)value;
					return asBool ? "1" : "0";
				}

				// specific handling for floating point types
				if ((value is decimal) || (value is float) || (value is double))
					return ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture.NumberFormat);

				// generic handling for all other value types
				return value.ToString();
			}

			// objects return null
			return null;
		}
	}
}
