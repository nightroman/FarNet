using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace PowerShellFar.Commands;

internal class BaseTaskCmdlet : PSCmdlet
{
	[Parameter(Position = 0, Mandatory = true)]
	public ScriptBlock Script
	{
		//! make unbound script
		set => _Script = ((ScriptBlockAst)value.Ast).GetScriptBlock();
		get => _Script;
	}
	ScriptBlock _Script = null!;

	protected Hashtable GetData() => (Hashtable)GetVariableValue(StartFarTaskCommand.NameData);

	protected VarDictionary GetVars() => new(SessionState.PSVariable);
}
