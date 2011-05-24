
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2011 FarNet Team
*/

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace FarNet.Works
{
	public abstract class ProxyAction : IModuleAction
	{
		ModuleActionAttribute _Attribute;
		string _ClassName;
		Type _ClassType;
		Guid _Id;
		readonly ModuleManager _Manager;
		protected ProxyAction(ModuleManager manager, EnumerableReader reader, ModuleActionAttribute attribute)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			_Manager = manager;
			_Attribute = attribute;

			_ClassName = (string)reader.Read();
			_Attribute.Name = (string)reader.Read();
			_Id = (Guid)reader.Read();
		}
		protected ProxyAction(ModuleManager manager, Guid id, ModuleActionAttribute attribute)
		{
			_Manager = manager;
			_Id = id;
			_Attribute = attribute;

			Init();
		}
		protected ProxyAction(ModuleManager manager, Type classType, Type attributeType)
		{
			if (classType == null)
				throw new ArgumentNullException("classType");
			if (attributeType == null)
				throw new ArgumentNullException("attributeType");

			_Manager = manager;
			_ClassType = classType;
			_Id = classType.GUID;

			object[] attrs;

			// ID: we have already got it, now ensure it is explicitely set
			attrs = _ClassType.GetCustomAttributes(typeof(GuidAttribute), false);
			if (attrs.Length == 0)
				throw new ModuleException(string.Format(null, "The Guid attribute should be set for the class '{0}'.", _ClassType.Name));

			// Module* attribure
			attrs = _ClassType.GetCustomAttributes(attributeType, false);
			if (attrs.Length == 0)
				throw new ModuleException(string.Format(null, "The '{0}' should be set for the class '{1}'.", attributeType.Name, _ClassType.Name));

			_Attribute = (ModuleActionAttribute)attrs[0];

			Init();

			if (_Attribute.Resources)
			{
				_Manager.CachedResources = true;
				string name = _Manager.GetString(_Attribute.Name);
				if (!string.IsNullOrEmpty(name))
					_Attribute.Name = name;
			}
		}
		protected ModuleActionAttribute Attribute
		{
			get { return _Attribute; }
		}
		internal ModuleAction GetInstance()
		{
			if (_ClassType == null)
			{
				_ClassType = _Manager.LoadAssembly().GetType(_ClassName, true, false);
				_ClassName = null;
			}

			return (ModuleAction)_Manager.CreateEntry(_ClassType);
		}
		void Init()
		{
			if (string.IsNullOrEmpty(_Attribute.Name))
				throw new ModuleException("Empty module action name is not valid.");
		}
		internal void Invoking()
		{
			_Manager.Invoking();
		}
		public override string ToString()
		{
			return string.Format(null, "{0} {1} {2} Name='{3}'", _Manager.ModuleName, Id, Kind, Name);
		}
		public virtual void Unregister()
		{
			Host.Instance.UnregisterProxyAction(this);
		}
		internal virtual void WriteCache(IList data)
		{
			data.Add(Kind);
			data.Add(ClassName);
			data.Add(Name);
			data.Add(_Id);
		}
		// Properties
		internal string ClassName
		{
			get
			{
				return _ClassType == null ? _ClassName : _ClassType.FullName;
			}
		}
		public virtual Guid Id
		{
			get { return _Id; }
		}
		public abstract ModuleItemKind Kind { get; }
		public virtual string Name
		{
			get { return _Attribute.Name; }
		}
		public IModuleManager Manager
		{
			get { return _Manager; }
		}
		internal abstract Hashtable SaveData();
		internal abstract void LoadData(Hashtable data);
	}
}
