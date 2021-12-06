
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public abstract class Host
	{
		public static Dictionary<Guid, IModuleAction> Actions { get; } = new();

		public static Host Instance
		{
			get { return _Instance_; }
			set
			{
				if (_Instance_ != null)
					throw new InvalidOperationException();

				_Instance_ = value;
			}
		}
		static Host _Instance_;

		public static HostState State
		{
			get => _State_;
			set => _State_ = value;
		}
		static HostState _State_;

		public abstract void RegisterProxyCommand(IModuleCommand info);
		public abstract void RegisterProxyDrawer(IModuleDrawer info);
		public abstract void RegisterProxyEditor(IModuleEditor info);
		public abstract void RegisterProxyTool(IModuleTool info);
		public abstract void UnregisterProxyAction(IModuleAction action);
		public abstract void UnregisterProxyTool(IModuleTool tool);
		public abstract void InvalidateProxyCommand();

		public static IEnumerable<IModuleTool> EnumTools()
		{
			foreach (IModuleAction action in Actions.Values)
			{
				if (action.Kind == ModuleItemKind.Tool)
					yield return (IModuleTool)action;
			}
		}

		public static IModuleTool[] GetTools(ModuleToolOptions option)
		{
			var list = new List<IModuleTool>(Actions.Count);
			foreach (IModuleAction action in Actions.Values)
			{
				if (action.Kind != ModuleItemKind.Tool)
					continue;

				IModuleTool tool = (IModuleTool)action;
				if (0 != (tool.Options & option))
					list.Add(tool);
			}
			return list.ToArray();
		}

	}

	public enum HostState
	{
		None,
		Loading,
		Loaded,
		Unloading,
		Unloaded
	}
}
