namespace FarNet.Works;
#pragma warning disable 1591

public abstract class Far2
{
	static Far2? _Host;

	public static Far2 Api
	{
		get => _Host!;
		set => _Host = _Host == null ? value : throw new InvalidOperationException();
	}

	public event EventHandler<QuittingEventArgs>? Quitting;

	public void OnQuit(QuittingEventArgs e) => Quitting?.Invoke(null, e);

	#region Modules
	public static Dictionary<Guid, IModuleAction> Actions { get; } = [];

	public abstract void RegisterProxyCommand(IModuleCommand info);

	public abstract void RegisterProxyDrawer(IModuleDrawer info);

	public abstract void RegisterProxyEditor(IModuleEditor info);

	public abstract void RegisterProxyTool(IModuleTool info);

	public abstract void UnregisterProxyAction(IModuleAction action);

	public abstract void UnregisterProxyTool(IModuleTool tool);

	public abstract void InvalidateProxyCommand();

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
		return [.. tools];
	}
	#endregion

	// Creates the module panel.
	public abstract IPanelWorks CreatePanel(Panel panel, Explorer explorer);

	// Waits for posted steps to be invoked.
	public abstract Task WaitSteps();

	// Posts macro and gets its wait handle.
	public abstract WaitHandle PostMacroWait(string macro);

	public abstract (IntPtr, int) IEditorLineText(IntPtr id, int line);
	public abstract void IEditorLineText(IntPtr id, int line, IntPtr p, int n);

	public abstract (IntPtr, int) ILineText(ILine line);
	public abstract void ILineText(ILine line, IntPtr p, int n);
}
