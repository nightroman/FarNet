namespace FarNet.Works;

abstract class ProxyAction : IModuleAction
{
	string? _ClassName;
	Type? _ClassType;
	readonly Guid _Id;
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

		var attr = Attribute.GetCustomAttribute(classType, attributeType)
			?? throw new ModuleException($"{_ClassType.FullName} must use {attributeType.FullName}.");

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

	//! Used compiled expressions here, not proven to be more effective.
	internal object GetInstance()
	{
		// resolve class name to its type
		if (_ClassType is null)
		{
			_ClassType = _Manager.LoadAssembly().GetType(_ClassName!, true, false);
			_ClassName = null;
		}

		// get new instance
		return Activator.CreateInstance(_ClassType!)!;
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
		Far2.Api.UnregisterProxyAction(this);
	}
}
