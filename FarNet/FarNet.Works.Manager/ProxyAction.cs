
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace FarNet.Works
{
	public abstract class ProxyAction : IModuleAction
	{
		string _ClassName;
		Type _ClassType;
		Guid _Id;
		readonly ModuleManager _Manager;
		protected ProxyAction(ModuleManager manager, EnumerableReader reader, ModuleActionAttribute attribute)
		{
			if (reader == null)
				throw new ArgumentNullException("reader");

			_Manager = manager;
			Attribute = attribute;

			_ClassName = (string)reader.Read();
			Attribute.Name = (string)reader.Read();
			_Id = (Guid)reader.Read();
		}
		protected ProxyAction(ModuleManager manager, Guid id, ModuleActionAttribute attribute)
		{
			_Manager = manager;
			_Id = id;
			Attribute = attribute;

			Initialize();
		}
		protected ProxyAction(ModuleManager manager, Type classType, Type attributeType)
		{
			_Manager = manager;
			_ClassType = classType ?? throw new ArgumentNullException();
			if (attributeType == null) throw new ArgumentNullException();

			object[] attrs;

			// Guid attribute. Do not Type.GUID, make sure Guid is used.
			attrs = _ClassType.GetCustomAttributes(typeof(GuidAttribute), false);
			if (attrs.Length == 0)
				throw new ModuleException($"Use '{typeof(GuidAttribute).FullName}' attribute for '{_ClassType.Name}'.");

			_Id = new Guid(((GuidAttribute)attrs[0]).Value);
			
			// Module* attribure
			attrs = _ClassType.GetCustomAttributes(attributeType, false);
			if (attrs.Length == 0)
				throw new ModuleException($"Use '{attributeType.FullName}' attribute for '{_ClassType.Name}' class.");

			Attribute = (ModuleActionAttribute)attrs[0];

			Initialize();

			if (Attribute.Resources)
			{
				_Manager.CachedResources = true;
				string name = _Manager.GetString(Attribute.Name);
				if (!string.IsNullOrEmpty(name))
					Attribute.Name = name;
			}
		}
		protected ModuleActionAttribute Attribute { get; }
		internal object GetInstance()
		{
			if (_ClassType == null)
			{
				_ClassType = _Manager.LoadAssembly().GetType(_ClassName, true, false);
				_ClassName = null;
			}

			return Activator.CreateInstance(_ClassType, false);
		}
		void Initialize()
		{
			if (string.IsNullOrEmpty(Attribute.Name))
				throw new ModuleException("Module action name cannot be empty.");
		}
		internal void Invoking()
		{
			_Manager.Invoking();
		}
		public override string ToString()
		{
			return $"{_Manager.ModuleName} {Id} {Kind} Name='{Name}'";
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
		public virtual Guid Id => _Id;
		public virtual string Name => Attribute.Name;
		public IModuleManager Manager => _Manager;
		internal string ClassName => _ClassType == null ? _ClassName : _ClassType.FullName;
		// Abstract
		public abstract ModuleItemKind Kind { get; }
		internal abstract Hashtable SaveData();
		internal abstract void LoadData(Hashtable data);
	}
}
