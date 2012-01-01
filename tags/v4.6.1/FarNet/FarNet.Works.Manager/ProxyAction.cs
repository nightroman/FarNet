
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

			Initialize();
		}
		protected ProxyAction(ModuleManager manager, Type classType, Type attributeType)
		{
			if (classType == null) throw new ArgumentNullException("classType");
			if (attributeType == null) throw new ArgumentNullException("attributeType");

			_Manager = manager;
			_ClassType = classType;

			object[] attrs;

			// Guid attribute; get it this way, not as Type.GUID, to be sure the attribute is applied
			attrs = _ClassType.GetCustomAttributes(typeof(GuidAttribute), false);
			if (attrs.Length == 0)
				throw new ModuleException(string.Format(null, "Apply the Guid attribute to the '{0}' class.", _ClassType.Name));

			_Id = new Guid(((GuidAttribute)attrs[0]).Value);
			
			// Module* attribure
			attrs = _ClassType.GetCustomAttributes(attributeType, false);
			if (attrs.Length == 0)
				throw new ModuleException(string.Format(null, "Apply the '{0}' attribute to the '{1}' class.", attributeType.Name, _ClassType.Name));

			_Attribute = (ModuleActionAttribute)attrs[0];

			Initialize();

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

			return (ModuleAction)ModuleManager.CreateEntry(_ClassType);
		}
		void Initialize()
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
			data.Add((int)Kind);
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
