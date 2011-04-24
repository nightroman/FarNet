
// http://msdn.microsoft.com/en-us/library/ms950764.aspx

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace FarNet.Tools
{
	class ObjectXPathProxy
	{
		static readonly ObjectXPathProxy[] _emptyElements = new ObjectXPathProxy[0];
		static readonly string[] _emptyNames = new string[0];
		readonly object _binding;
		public object Binding { get { return _binding; } }
		readonly ObjectXPathContext _context;
		readonly string _name;
		readonly ObjectXPathProxy _parent;
		bool _activated;
		Dictionary<string, string> _attributes;
		string[] _attributeKeys;
		List<ObjectXPathProxy> _elements;
		Dictionary<string, ObjectXPathProxy> _elemDict;
		public ObjectXPathProxy(object binding, ObjectXPathContext context) : this(binding, context, null, null) {}
		ObjectXPathProxy(object binding, ObjectXPathContext context, string name, ObjectXPathProxy parent)
		{
			if (binding == null) throw new ArgumentNullException("binding");
			if (context == null) throw new ArgumentNullException("context");
			
			_binding = binding;
			_context = context;
			_parent = parent;
			
			if (string.IsNullOrEmpty(name))
			{
				var info = binding as IXmlInfo;
				if (info == null)
				{
					var type = binding.GetType();
					name = binding.GetType().Name;
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
		public ObjectXPathProxy Parent { get { return _parent; } }
		public string Value
		{
			get
			{
				if (HasText)
					return CultureSafeToString(_binding);

				return string.Empty;
			}
		}
		public bool HasAttributes
		{
			get
			{
				Activate();

				return (_attributes != null);
			}
		}
		public bool HasChildren
		{
			get
			{
				Activate();

				return (_elements != null) || HasText;
			}
		}
		public bool HasText
		{
			get
			{
				Type t = _binding.GetType();

				return (t.IsValueType || t == typeof(string));
			}
		}
		public IList<string> AttributeKeys
		{
			get
			{
				Activate();

				if (_attributeKeys != null)
					return _attributeKeys;
				else
					return _emptyNames;
			}
		}
		public string GetAttributeValue(string name)
		{
			Activate();

			string value;
			if (_attributes != null && _attributes.TryGetValue(name, out value))
				return value;

			return string.Empty;
		}
		public IList<ObjectXPathProxy> Elements
		{
			get
			{
				Activate();

				if (_elements != null)
					return _elements;
				else
					return _emptyElements;
			}
		}
		public void AddSpecialName(string key, string value)
		{
			Activate();

			if (_attributes == null)
				_attributes = new Dictionary<string, string>();

			_attributes["*" + key] = value;

			_attributeKeys = new string[_attributes.Count];
			_attributes.Keys.CopyTo(_attributeKeys, 0);
		}
		void Activate()
		{
			if (_activated)
				return;

			if (_context.Stopping != null && _context.Stopping(null))
				return;

			lock (this)
			{
				if (_activated)
					return;

				if (_binding is ValueType || _binding is string) //rvk "Linear types". Perhaps we need more.
				{
					// no attributes or children
				}
				else if (_binding is SuperFile)
				{
					ActivateSuperFile();
				}
				else if (_binding is IDictionary) //! before ICollection
				{
					ActivateDictionary();
				}
				else if (_binding is ICollection) //! after IDictionary
				{
					ActivateCollection();
				}
				else
				{
					ActivateSimple();
				}

				_activated = true;
			}
		}
		void ActivateDictionary()
		{
			var elements = new List<ObjectXPathProxy>();

			_elemDict = new Dictionary<string, ObjectXPathProxy>();

			foreach (DictionaryEntry entry in (IDictionary)_binding)
			{
				if (entry.Value == null)
					continue;

				var item = new ObjectXPathProxy(entry.Value, _context, null, this); //??????

				elements.Add(item);

				item.AddSpecialName("key", entry.Key.ToString());

				_elemDict[entry.Key.ToString()] = item;
			}

			_elements = (elements.Count != 0) ? elements : null;
		}
		void ActivateCollection()
		{
			_elements = new List<ObjectXPathProxy>();

			foreach (object val in (ICollection)_binding)
			{
				if (val == null)
					continue;

				_elements.Add(new ObjectXPathProxy(val, _context, null, this));
			}

			if (_elements.Count == 0)
				_elements = null;
		}
		void ActivateSimple()
		{
			var attributes = new Dictionary<string, string>();

			var info = _binding as IXmlInfo;
			if (info != null)
			{
				foreach (var kv in info.XmlAttributes())
					attributes.Add(kv.Key.ToString(), CultureSafeToString(kv.Value));
			}
			else
			{
				var elements = new List<ObjectXPathProxy>();

				foreach (PropertyInfo pi in _binding.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					// get the value
					object val = pi.GetValue(_binding, null);

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
						attributes.Add(GetAtomicString(pi.Name), str);
					else
						elements.Add(new ObjectXPathProxy(val, _context, pi.Name, this));

					_elements = (elements.Count != 0) ? elements : null;
				}
			}

			_attributes = (attributes.Count != 0) ? attributes : null;
			if (_attributes != null)
			{
				_attributeKeys = new string[_attributes.Count];
				_attributes.Keys.CopyTo(_attributeKeys, 0);
			}
		}
		void ActivateSuperFile()
		{
			var file = (SuperFile)_binding; //??????
			
			_attributes = new Dictionary<string, string>();
			foreach(var kv in file.XmlAttributes())
				_attributes.Add(kv.Key.ToString(), CultureSafeToString(kv.Value));

			_attributeKeys = new string[_attributes.Count];
			_attributes.Keys.CopyTo(_attributeKeys, 0);

			if (file.IsDirectory)
			{
				// progress
				if (_context.IncrementDirectoryCount != null)
					_context.IncrementDirectoryCount(1);
				
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
					_elements = new List<ObjectXPathProxy>();
					
					var argsFiles = new GetFilesEventArgs(ExplorerModes.Find);
					foreach (var it in explorer.GetFiles(argsFiles))
					{
						// filter out a leaf
						if (_context.Filter != null && !it.IsDirectory && !_context.Filter(explorer, it))
							continue;

						// add
						_elements.Add(new ObjectXPathProxy(new SuperFile(explorer, it), _context, null, this));
					}

					if (_elements.Count == 0)
						_elements = null;
				}
			}
		}
		string GetAtomicString(string array)
		{
			return _context.NameTable.Get(array) ?? _context.NameTable.Add(array);
		}
		static string CultureSafeToString(object value)
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
					DateTime dt = (DateTime)value;
					return dt.ToString((dt.TimeOfDay.Ticks > 0 ? "yyyy-MM-dd HH:mm:ss" : "yyyy-MM-dd"), null);
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
