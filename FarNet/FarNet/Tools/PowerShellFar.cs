using System.Collections;

namespace FarNet.Tools;

/// <summary>
/// PowerShellFar module interop.
/// </summary>
public static class PowerShellFar
{
	private static readonly Lazy<IModuleManager> _psfManager = new(Far.Api.GetModuleManager("PowerShellFar"));
	private static readonly Lazy<object> _psfRunspace = new(_psfManager.Value.Interop("Runspace", null));
	private static readonly Lazy<Func<string, object[]?, object[]>> _psfInvokeScript = new((Func<string, object[]?, object[]>)_psfManager.Value.Interop("InvokeScriptArguments", null));

	/// <summary>
	/// Gets the main runspace.
	/// </summary>
	public static object Runspace => _psfRunspace.Value;

	/// <summary>
	/// Invokes PowerShell script code.
	/// </summary>
	/// <param name="script">The script code to invoke.</param>
	/// <param name="args">Optional script arguments.</param>
	/// <returns>Script output.</returns>
	public static object[] Invoke(string script, object[]? args = null)
	{
		return _psfInvokeScript.Value(script, args);
	}

	/// <summary>
	/// Invokes script file or code by Start-FarTask.
	/// </summary>
	/// <param name="script">The script file or code to invoke.</param>
	/// <param name="parameters">Optional script parameters, e.g. <c>Hashtable</c> or <c>Dictionary[string, object]</c>.</param>
	/// <returns>Script output.</returns>
	public static Task<object[]> InvokeAsync(string script, IDictionary? parameters = null)
	{
		parameters ??= new Hashtable();
		var r = Invoke("param($Script, $Parameters) Start-FarTask $Script @Parameters -AsTask", [script, parameters]);
		return (Task<object[]>)r[0];
	}
}
