
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace FarNet.Works
{
	public abstract class ProxyAction : IModuleAction
	{
		string _ClassName;
		Type _ClassType;
		Guid _Id;
		Func<object> _Constructor;
		readonly ModuleManager _Manager;
		protected ModuleActionAttribute Attribute { get; }

		// Properties
		public virtual Guid Id => _Id;
		public virtual string Name => Attribute.Name;
		public IModuleManager Manager => _Manager;
		internal string ClassName => _ClassType == null ? _ClassName : _ClassType.FullName;

		// Abstract
		public abstract ModuleItemKind Kind { get; }
		internal abstract Hashtable SaveConfig();
		internal abstract void LoadConfig(Hashtable data);

		internal ProxyAction(ModuleManager manager, BinaryReader reader, ModuleActionAttribute attribute)
		{
			_Manager = manager;
			Attribute = attribute;

			// [1]
			_ClassName = reader.ReadString();
			// [2]
			Attribute.Name = reader.ReadString();
			// [3]
			_Id = new Guid(reader.ReadBytes(16));
		}

		internal virtual void WriteCache(BinaryWriter writer)
		{
			// [1]
			writer.Write(ClassName);
			// [2]
			writer.Write(Name);
			// [3]
			writer.Write(_Id.ToByteArray());
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
				string name = _Manager.GetString(Attribute.Name);
				if (!string.IsNullOrEmpty(name))
					Attribute.Name = name;
			}
		}

		internal object GetInstance()
		{
			// resolve class name to its type
			if (_ClassType is null)
			{
				_ClassType = _Manager.LoadAssembly().GetType(_ClassName, true, false);
				_ClassName = null;
			}

			// compile its default constructor
			// Faster than Activator.CreateInstance for 2+ calls.
			// For singletons still use Activator.CreateInstance.
			if (_Constructor is null)
			{
				_Constructor = Expression.Lambda<Func<object>>(Expression.New(_ClassType)).Compile();
			}

			// get new instance
			return _Constructor();
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
	}
}
