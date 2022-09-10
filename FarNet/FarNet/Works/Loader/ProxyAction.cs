
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;
using System.Linq.Expressions;

namespace FarNet.Works;

abstract class ProxyAction : IModuleAction
{
	string? _ClassName;
	Type? _ClassType;
	readonly Guid _Id;
	Func<object>? _Constructor;
	readonly ModuleManager _Manager;
	protected ModuleActionAttribute ActionAttribute { get; }

	// Properties
	public virtual Guid Id => _Id;
	public virtual string Name => ActionAttribute.Name;
	public IModuleManager Manager => _Manager;
	internal string ClassName => _ClassType == null ? _ClassName! : _ClassType.FullName!;

	// Abstract
	public abstract ModuleItemKind Kind { get; }

	internal ProxyAction(ModuleManager manager, BinaryReader reader, ModuleActionAttribute attribute)
	{
		_Manager = manager;
		ActionAttribute = attribute;

		// [1]
		_ClassName = reader.ReadString();
		// [2]
		ActionAttribute.Name = reader.ReadString();
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
		ActionAttribute = attribute;

		Initialize();
	}

	protected ProxyAction(ModuleManager manager, Type classType, Type attributeType)
	{
		_Manager = manager;
		_ClassType = classType;

		var attr = Attribute.GetCustomAttribute(classType, attributeType);
		if (attr is null)
			throw new ModuleException($"{_ClassType.FullName} must use {attributeType.FullName}.");

		ActionAttribute = (ModuleActionAttribute)attr;
		if (!Guid.TryParse(ActionAttribute.Id, out _Id))
			throw new ModuleException($"{_ClassType.FullName}: {attributeType.FullName} uses invalid GUID as Id.");

		Initialize();

		if (ActionAttribute.Resources)
		{
			var name = _Manager.GetString(ActionAttribute.Name);
			if (!string.IsNullOrEmpty(name))
				ActionAttribute.Name = name;
		}
	}

	internal object GetInstance()
	{
		// resolve class name to its type
		if (_ClassType is null)
		{
			_ClassType = _Manager.LoadAssembly().GetType(_ClassName!, true, false);
			_ClassName = null;
		}

		// compile its default constructor
		// Faster than Activator.CreateInstance for 2+ calls.
		// For singletons still use Activator.CreateInstance.
		_Constructor ??= Expression.Lambda<Func<object>>(Expression.New(_ClassType!)).Compile();

		// get new instance
		return _Constructor();
	}

	void Initialize()
	{
		if (string.IsNullOrEmpty(ActionAttribute.Name))
			throw new ModuleException($"{_ClassType!.FullName} must set action Name.");
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
