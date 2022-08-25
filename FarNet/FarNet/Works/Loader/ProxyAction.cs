
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace FarNet.Works;

abstract class ProxyAction : IModuleAction
{
	string _ClassName;
	Type _ClassType;
	readonly Guid _Id;
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
		_ClassType = classType ?? throw new ArgumentNullException(nameof(classType));
		if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));

		object[] attrs;

		// Module* attribure
		attrs = _ClassType.GetCustomAttributes(attributeType, false);
		if (attrs.Length == 0)
			throw new ModuleException($"{_ClassType.FullName} must use {attributeType.FullName}.");

		Attribute = (ModuleActionAttribute)attrs[0];

		if (Attribute.Id is null)
		{
			// Legacy Guid attribute.
			attrs = _ClassType.GetCustomAttributes(typeof(GuidAttribute), false);
			if (attrs.Length == 0)
				throw new ModuleException($"{_ClassType.FullName} must specify {attributeType.FullName} Id.");

			_Id = new Guid(((GuidAttribute)attrs[0]).Value);
		}
		else
		{
			if (!Guid.TryParse(Attribute.Id, out _Id))
				throw new ModuleException($"{_ClassType.FullName}: {attributeType.FullName} uses invalid GUID as Id.");
		}

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
		_Constructor ??= Expression.Lambda<Func<object>>(Expression.New(_ClassType)).Compile();

		// get new instance
		return _Constructor();
	}

	void Initialize()
	{
		if (string.IsNullOrEmpty(Attribute.Name))
			throw new ModuleException($"{_ClassType.FullName} must set action Name.");
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
