
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet.Works;

///
public abstract class Host
{
	static Host _Host;

	///
	public static Host Instance
	{
		get => _Host;
		set => _Host = _Host == null ? value : throw new InvalidOperationException();
	}

	///
	public static Dictionary<Guid, IModuleAction> Actions { get; } = new();

	///
	public abstract void RegisterProxyCommand(IModuleCommand info);

	///
	public abstract void RegisterProxyDrawer(IModuleDrawer info);

	///
	public abstract void RegisterProxyEditor(IModuleEditor info);

	///
	public abstract void RegisterProxyTool(IModuleTool info);

	///
	public abstract void UnregisterProxyAction(IModuleAction action);

	///
	public abstract void UnregisterProxyTool(IModuleTool tool);

	///
	public abstract void InvalidateProxyCommand();

	///
	public static List<IModuleTool> ListTools()
	{
		var tools = new List<IModuleTool>(Actions.Count);
		foreach (IModuleAction action in Actions.Values)
			if (action.Kind == ModuleItemKind.Tool)
				tools.Add((IModuleTool)action);
		return tools;
	}

	///
	public static IModuleTool[] GetTools(ModuleToolOptions option)
	{
		var tools = new List<IModuleTool>(Actions.Count);
		foreach (var action in Actions.Values)
		{
			if (action.Kind != ModuleItemKind.Tool)
				continue;

			var tool = (IModuleTool)action;
			if (0 != (tool.Options & option))
				tools.Add(tool);
		}
		return tools.ToArray();
	}
}
