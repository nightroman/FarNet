/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FarNet.Works
{
	public abstract class ProxyAction : IModuleAction
	{
		ModuleActionAttribute _Attribute;
		string _ClassName;
		Type _ClassType;
		Guid _Id;
		readonly ModuleManager _ModuleManager;

		protected ProxyAction(ModuleManager manager, EnumerableReader reader, ModuleActionAttribute attribute)
		{
			_ModuleManager = manager;
			_Attribute = attribute;

			_ClassName = reader.Read();
			_Attribute.Name = reader.Read();
			_Id = new Guid(reader.Read());
		}

		protected ProxyAction(ModuleManager manager, Guid id, ModuleActionAttribute attribute)
		{
			_ModuleManager = manager;
			_Id = id;
			_Attribute = attribute;

			Init();
		}

		protected ProxyAction(ModuleManager manager, Type classType, Type attributeType)
		{
			_ModuleManager = manager;
			_ClassType = classType;
			_Id = classType.GUID;

			object[] attrs;

			// ID: we have already got it, now ensure it is explicitely set
			attrs = _ClassType.GetCustomAttributes(typeof(GuidAttribute), false);
			if (attrs.Length == 0)
				throw new ModuleException(Invariant.Format("The 'GuidAttribute' should be set for the class '{0}'.", _ClassType.Name));

			// Module* attribure
			attrs = _ClassType.GetCustomAttributes(attributeType, false);
			if (attrs.Length == 0)
				throw new ModuleException(Invariant.Format("The '{0}' should be set for the class '{1}'.", attributeType.Name, _ClassType.Name));

			_Attribute = (ModuleActionAttribute)attrs[0];

			Init();

			if (_Attribute.Resources)
			{
				_ModuleManager.CachedResources = true;
				string name = _ModuleManager.GetString(_Attribute.Name);
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
				_ClassType = _ModuleManager.AssemblyInstance.GetType(_ClassName, true, false);
				_ClassName = null;
			}

			return (ModuleAction)_ModuleManager.CreateEntry(_ClassType);
		}

		void Init()
		{
			if (string.IsNullOrEmpty(_Attribute.Name))
				throw new ModuleException("Empty module action name is not valid.");
		}

		internal void Invoking()
		{
			_ModuleManager.Invoking();
		}

		public override string ToString()
		{
			return Invariant.Format("{0} {1} Name='{2}'", Key, Kind, Name);
		}

		public virtual void Unregister()
		{
			Host.Instance.UnregisterProxyAction(this);
		}


		internal virtual void WriteCache(List<string> data)
		{
			data.Add(Kind.ToString());
			data.Add(ClassName);
			data.Add(Name);
			data.Add(_Id.ToString());
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

		internal string Key
		{
			get
			{
				return ModuleName + "\\" + _Id;
			}
		}

		public abstract ModuleItemKind Kind { get; }

		internal ModuleManager Manager
		{
			get { return _ModuleManager; }
		}

		public virtual string ModuleName
		{
			get
			{
				return _ModuleManager.ModuleName;
			}
		}

		public virtual string Name
		{
			get { return _Attribute.Name; }
		}
	}
}
