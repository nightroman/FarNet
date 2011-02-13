
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FarNet.Works
{
	public abstract class Host
	{
		public static Dictionary<Guid, IModuleAction> Actions { get { return _Actions; } }
		static readonly Dictionary<Guid, IModuleAction> _Actions = new Dictionary<Guid, IModuleAction>();

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
			get { return _State_; }
			set
			{
				Log.Source.TraceInformation("Host state has changed from {0} to {1}", _State_, value);

				_State_ = value;
			}
		}
		static HostState _State_;

		public abstract object LoadFarNetValue(string keyPath, string valueName, object defaultValue);
		public abstract void SaveFarNetValue(string keyPath, string valueName, object value);
		public abstract IRegistryKey OpenModuleKey(string name, bool writable);
		public abstract IRegistryKey OpenCacheKey(bool writable);

		public abstract void RegisterProxyCommand(IModuleCommand info);
		public abstract void RegisterProxyEditor(IModuleEditor info);
		public abstract void RegisterProxyFiler(IModuleFiler info);
		public abstract void RegisterProxyTool(IModuleTool info);
		public abstract void UnregisterProxyAction(IModuleAction action);
		public abstract void UnregisterProxyTool(IModuleTool tool);
		public abstract void InvalidateProxyCommand();

		public static IEnumerable<IModuleTool> EnumTools()
		{
			foreach (IModuleAction action in _Actions.Values)
			{
				if (action.Kind == ModuleItemKind.Tool)
					yield return (IModuleTool)action;
			}
		}

		public static IModuleTool[] GetTools(ModuleToolOptions option)
		{
			var list = new List<IModuleTool>(_Actions.Count);
			foreach (IModuleAction action in _Actions.Values)
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
